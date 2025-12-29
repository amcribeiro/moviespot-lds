using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Genres;

namespace MovieSpot.Services.Movies
{
    /// <summary>
    /// Movie business logic service that orchestrates local persistence (EF Core)
    /// with the external TMDB API via <see cref="ITMDBAPIService"/>.
    /// </summary>
    /// <remarks>
    /// Responsibilities:
    /// <list type="number">
    /// <item>Read/write local <see cref="Movie"/> entities (with genres included).</item>
    /// <item>Fetch trending and detailed movie information from TMDB.</item>
    /// <item>Upsert TMDB movies into the local database via <see cref="SyncMovies"/>.</item>
    /// </list>
    /// </remarks>
    public sealed class MovieService : IMovieService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITMDBAPIService _tmdb;
        private readonly IGenreService _genreService;

        /// <summary>
        /// Creates an instance of <see cref="MovieService"/>.
        /// </summary>
        /// <param name="db">The EF Core DbContext used for persistence.</param>
        /// <param name="tmdb">External TMDB abstraction used to retrieve movie data.</param>
        /// <param name="genreService">Genre service (reserved for future movie-genre sync).</param>
        /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
        public MovieService(ApplicationDbContext db,
                            ITMDBAPIService tmdb,
                            IGenreService genreService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _tmdb = tmdb ?? throw new ArgumentNullException(nameof(tmdb));
            _genreService = genreService ?? throw new ArgumentNullException(nameof(genreService));
        }

        /// <summary>
        /// Retrieves all local movies including their genres (eager-loaded).
        /// </summary>
        /// <returns>List of <see cref="Movie"/> entities with <see cref="Movie.MovieGenres"/> populated.</returns>
        public List<Movie> GetMovies()
            => _db.Movie
                  .Include(m => m.MovieGenres)
                  .ThenInclude(mg => mg.Genre)
                  .ToList();

        /// <summary>
        /// Gets a single local movie by its identifier.
        /// </summary>
        /// <param name="id">Movie identifier (must be &gt; 0).</param>
        /// <returns>The <see cref="Movie"/> entity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is not positive.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the movie does not exist.</exception>
        public Movie GetMovie(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var movie = _db.Movie
                           .Include(m => m.MovieGenres)
                           .ThenInclude(mg => mg.Genre)
                           .FirstOrDefault(m => m.Id == id);
            return movie ?? throw new KeyNotFoundException($"Movie {id} not found.");
        }

        /// <summary>
        /// Synchronizes trending movies from TMDB into the local database via upsert.
        /// </summary>
        /// <remarks>
        /// For each TMDB movie, the method either inserts a new <see cref="Movie"/> (if missing)
        /// or updates an existing one with fresh data (title, description, duration, language, release date, country, poster).
        /// Genre synchronization is left as a TODO for a future iteration.
        /// </remarks>
        /// <summary>
        /// Synchronizes existing movies in the database with the latest trending movies from TMDB.
        /// </summary>
        /// <remarks>
        /// Updates only movies already present in the local database,
        /// including their MovieGenre relationships if needed.
        /// Does not create or remove any unrelated entities.
        /// </remarks>
        /// <summary>
        /// Fully synchronizes the local database with the trending movies from TMDB.
        /// </summary>
        /// <remarks>
        /// - Adds movies that exist in TMDB but not locally.  
        /// - Updates existing movies if any field or genre has changed.  
        /// - Removes movies that are no longer trending in TMDB.  
        /// - Synchronizes MovieGenre relations (no new genres are created).
        /// </remarks>
        public async Task SyncMovies()
        {
            var trendingMovies = await _tmdb.GetTrendingMoviesWithDetails("week");

            var existingMovies = GetMovies().ToDictionary(m => m.Id);

            var apiMovieIds = trendingMovies.Select(m => m.Id).ToHashSet();

            var moviesToRemove = existingMovies.Values
                .Where(m => !apiMovieIds.Contains(m.Id))
                .ToList();

            if (moviesToRemove.Count > 0)
            {
                _db.Movie.RemoveRange(moviesToRemove);
            }

            foreach (var apiMovie in trendingMovies)
            {
                apiMovie.Title ??= string.Empty;
                apiMovie.Overview ??= string.Empty;
                apiMovie.OriginalLanguage ??= string.Empty;
                apiMovie.ReleaseDate ??= string.Empty;
                apiMovie.PosterPath ??= string.Empty;
                apiMovie.OriginCountry ??= new List<string>();
                apiMovie.Genres ??= new List<Genre>();

                if (!existingMovies.TryGetValue(apiMovie.Id, out var existing))
                {
                    var newMovie = new Movie
                    {
                        Id = apiMovie.Id,
                        Title = apiMovie.Title,
                        Description = apiMovie.Overview,
                        Duration = apiMovie.Runtime,
                        Language = apiMovie.OriginalLanguage,
                        ReleaseDate = ParseDate(apiMovie.ReleaseDate),
                        Country = apiMovie.OriginCountry.FirstOrDefault() ?? string.Empty,
                        PosterPath = apiMovie.PosterPath,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    foreach (var genre in apiMovie.Genres)
                    {
                        newMovie.MovieGenres.Add(new MovieGenre
                        {
                            MovieId = apiMovie.Id,
                            GenreId = genre.Id
                        });
                    }

                    _db.Movie.Add(newMovie);
                    continue;
                }

                bool requiresUpdate = false;

                if (existing.Title != apiMovie.Title)
                {
                    existing.Title = apiMovie.Title;
                    requiresUpdate = true;
                }

                if (existing.Description != apiMovie.Overview)
                {
                    existing.Description = apiMovie.Overview;
                    requiresUpdate = true;
                }

                if (apiMovie.Runtime > 0 && existing.Duration != apiMovie.Runtime)
                {
                    existing.Duration = apiMovie.Runtime;
                    requiresUpdate = true;
                }

                if (existing.Language != apiMovie.OriginalLanguage)
                {
                    existing.Language = apiMovie.OriginalLanguage;
                    requiresUpdate = true;
                }

                var parsedDate = ParseDate(apiMovie.ReleaseDate);
                if (parsedDate != default && existing.ReleaseDate != parsedDate)
                {
                    existing.ReleaseDate = parsedDate;
                    requiresUpdate = true;
                }

                var apiCountry = apiMovie.OriginCountry.FirstOrDefault() ?? string.Empty;
                if (existing.Country != apiCountry)
                {
                    existing.Country = apiCountry;
                    requiresUpdate = true;
                }

                if (existing.PosterPath != apiMovie.PosterPath)
                {
                    existing.PosterPath = apiMovie.PosterPath;
                    requiresUpdate = true;
                }

                var existingGenreIds = existing.MovieGenres.Select(mg => mg.GenreId).OrderBy(id => id).ToList();
                var apiGenreIds = apiMovie.Genres.Select(g => g.Id).OrderBy(id => id).ToList();

                if (!existingGenreIds.SequenceEqual(apiGenreIds))
                {
                    requiresUpdate = true;

                    var currentLinks = _db.MovieGenre.Where(mg => mg.MovieId == existing.Id).ToList();
                    _db.MovieGenre.RemoveRange(currentLinks);

                    foreach (var genreId in apiGenreIds)
                    {
                        _db.MovieGenre.Add(new MovieGenre
                        {
                            MovieId = existing.Id,
                            GenreId = genreId
                        });
                    }
                }

                if (requiresUpdate)
                {
                    existing.UpdatedAt = DateTime.UtcNow;
                    _db.Movie.Update(existing);
                }
            }

            _db.SaveChanges();
        }

        /// <summary>
        /// Adds a new movie to the local database.
        /// </summary>
        /// <param name="movie">The entity to persist.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="movie"/> is null.</exception>
        public void AddMovie(Movie movie)
        {
            if (movie is null) throw new ArgumentNullException(nameof(movie));
            _db.Movie.Add(movie);
            _db.SaveChanges();
        }

        /// <summary>
        /// Removes a movie by its identifier.
        /// </summary>
        /// <param name="id">Movie identifier (must be &gt; 0).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is not positive.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the entity is not found.</exception>
        public void RemoveMovie(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var m = _db.Movie.Find(id) ?? throw new KeyNotFoundException($"Movie {id} not found.");
            _db.Movie.Remove(m);
            _db.SaveChanges();
        }

        public List<MovieSessionCountDto> GetSessionCountsPerMovie()
        {
            return _db.Movie
                .Select(m => new MovieSessionCountDto
                {
                    MovieTitle = m.Title,
                    SessionCount = m.Sessions.Count() // O EF faz o Count() nas sessões associadas
                })
                .OrderByDescending(x => x.SessionCount)
                .ToList();
        }

        public List<MovieRevenueDto> GetRevenuePerMovie()
        {
            return _db.Movie
                .Select(m => new MovieRevenueDto
                {
                    MovieTitle = m.Title,
                    TotalRevenue = m.Sessions
                        .SelectMany(s => s.Bookings)
                        .Where(b => b.Status == true)
                        .Sum(b => b.TotalAmount)
                })
                .OrderByDescending(r => r.TotalRevenue)
                .ToList();
        }

        public List<PopularMovieDto> GetMostPopularMovies(int count = 5)
        {
            return _db.Movie
                .Select(m => new PopularMovieDto
                {
                    MovieTitle = m.Title,
                    // Movie -> Sessions -> Bookings -> BookingSeats
                    // Conta o total de lugares (bilhetes) vendidos para este filme
                    TicketsSold = m.Sessions
                        .SelectMany(s => s.Bookings)
                        .Where(b => b.Status == true)
                        .SelectMany(b => b.BookingSeats)
                        .Count(),

                    // Receita total desse filme
                    TotalRevenue = m.Sessions
                        .SelectMany(s => s.Bookings)
                        .Where(b => b.Status == true)
                        .Sum(b => b.TotalAmount)
                })
                .OrderByDescending(m => m.TicketsSold)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Parses a date string into a <see cref="DateTime"/> (date-only).
        /// Returns <see cref="DateTime.UtcNow"/>.Date when parsing fails.
        /// </summary>
        private static DateTime ParseDate(string s)
            => DateTime.TryParse(s, out var d) ? d.Date : DateTime.UtcNow.Date;
    }
}
