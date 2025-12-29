using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using Newtonsoft.Json.Linq;

namespace MovieSpot.Services.Genres
{
    /// <summary>
    /// Service responsible for managing movie genres, including synchronization
    /// with the TMDB external API and interaction with the local database.
    /// </summary>
    public class GenreService : IGenreService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenreService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="httpClientFactory">The HTTP client factory used to send API requests.</param>
        /// <param name="configuration">The application configuration instance for accessing API keys and URLs.</param>
        public GenreService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Fetches all available movie genres from the TMDB API.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Genre"/> objects retrieved from the TMDB API.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request to TMDB fails or returns a non-success status code.
        /// </exception>
        /// <exception cref="JsonReaderException">
        /// Thrown when the JSON response from TMDB cannot be parsed correctly.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the TMDB response structure is invalid or missing the expected "genres" key.
        /// </exception>
        public async Task<List<Genre>> GetAllGenresFromApiAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var apiKey = _configuration["TMDB:ApiKey"];
            var baseUrl = _configuration["TMDB:BaseUrl"];
            var url = $"{baseUrl}/genre/movie/list";

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch genres from TMDB. Status: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            var parsed = JObject.Parse(json);

            var genresToken = parsed["genres"];
            if (genresToken == null)
                throw new InvalidOperationException("TMDB API response did not contain a 'genres' field.");

            var genres = genresToken
                .Select(g => new Genre
                {
                    Id = (int)g["id"],
                    Name = string.IsNullOrWhiteSpace(g["name"]?.ToString())
    ? "Unknown"
    : g["name"]!.ToString()
                })
                .ToList();

            return genres;
        }

        /// <summary>
        /// Retrieves all genres currently stored in the local database.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Genre"/> objects stored locally.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no genres exist in the database.
        /// </exception>
        public List<Genre> GetAllGenresFromDb()
        {
            var genres = _context.Genre.ToList();

            if (!genres.Any())
                throw new InvalidOperationException("No genres found in the local database.");

            return genres;
        }

        /// <summary>
        /// Retrieves a specific genre from the local database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the genre.</param>
        /// <returns>
        /// The <see cref="Genre"/> matching the provided ID.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided ID is less than or equal to zero.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no genre exists with the specified ID.
        /// </exception>
        public Genre GetGenreById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Genre ID must be greater than zero.");

            var genre = _context.Genre.Find(id);

            if (genre == null)
                throw new KeyNotFoundException($"Genre with ID {id} was not found.");

            return genre;
        }

        /// <summary>
        /// Synchronizes movie genres from the TMDB API with the local database.
        /// Adds new genres and updates existing ones when necessary.
        /// </summary>
        /// <remarks>
        /// This method should be executed periodically (e.g., weekly) by a background service
        /// to keep the local data consistent with the external TMDB API.
        /// </remarks>
        /// <exception cref="HttpRequestException">
        /// Thrown when there is an issue fetching data from the TMDB API.
        /// </exception>
        /// <exception cref="DbUpdateException">
        /// Thrown when saving data to the local database fails.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the API response is invalid or contains inconsistent data.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown for any unexpected errors during synchronization.
        /// </exception>
        public async Task SyncGenresAsync()
        {
            try
            {
                var apiGenres = await GetAllGenresFromApiAsync();

                foreach (var genre in apiGenres)
                {
                    var existing = await _context.Genre.FindAsync(genre.Id);

                    if (existing == null)
                    {
                        _context.Genre.Add(genre);
                    }
                    else if (existing.Name != genre.Name)
                    {
                        existing.Name = genre.Name;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to synchronize genres with the local database.", ex);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred during genre synchronization.", ex);
            }
        }
        public List<GenreStatDto> GetGenreStatistics()
        {
            return _context.Genre
                .Select(g => new GenreStatDto
                {
                    GenreName = g.Name,
                    // Quantos filmes existem deste género
                    MoviesCount = g.MovieGenres.Count(),

                    // Quantas sessões agendadas existem para filmes deste género
                    // Navegação: Genre -> MovieGenres -> Movie -> Sessions
                    SessionsCount = g.MovieGenres
                        .SelectMany(mg => mg.Movie.Sessions)
                        .Count()
                })
                .OrderByDescending(x => x.SessionsCount)
                .ToList();
        }
    }
}
