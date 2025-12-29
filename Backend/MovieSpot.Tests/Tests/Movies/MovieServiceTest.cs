using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Genres;
using MovieSpot.Services.Movies;
using Xunit;

namespace MovieSpot.Tests.Services.Movies
{
    public class MovieServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ITMDBAPIService> _tmdbMock = new();
        private readonly Mock<IGenreService> _genreServiceMock = new();
        private readonly MovieService _service;

        public MovieServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _service = new MovieService(_context, _tmdbMock.Object, _genreServiceMock.Object);
        }

        #region SeedData
        private void SeedData()
        {
            var gAction = new Genre { Id = 1, Name = "Action" };
            var gDrama = new Genre { Id = 2, Name = "Drama" };
            var gThrill = new Genre { Id = 3, Name = "Thriller" };

            _context.Genre.AddRange(gAction, gDrama, gThrill);

            var m1 = new Movie
            {
                Id = 10,
                Title = "Alpha",
                Description = "Alpha desc",
                Duration = 120,
                ReleaseDate = new DateTime(2024, 1, 1),
                Language = "en",
                Country = "US",
                PosterPath = "/a.jpg"
            };
            var m2 = new Movie
            {
                Id = 11,
                Title = "Beta",
                Description = "Beta desc",
                Duration = 95,
                ReleaseDate = new DateTime(2023, 5, 10),
                Language = "pt",
                Country = "PT",
                PosterPath = "/b.jpg"
            };

            _context.Movie.AddRange(m1, m2);

            _context.MovieGenre.AddRange(
                new MovieGenre { MovieId = 10, GenreId = 1 },
                new MovieGenre { MovieId = 11, GenreId = 2 }
            );

            _context.SaveChanges();
        }

        #endregion

        #region GetMovies()

        [Fact]
        public void GetMovies_WhenMoviesExist_ReturnsAll()
        {
            SeedData();

            var list = _service.GetMovies();

            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            Assert.Contains(list, m => m.Id == 10);
            Assert.Contains(list, m => m.Id == 11);

            var alpha = list.First(m => m.Id == 10);
            Assert.Contains(alpha.MovieGenres, mg => mg.GenreId == 1);
        }

        [Fact]
        public void GetMovies_WhenEmpty_ReturnsEmptyList()
        {
            var list = _service.GetMovies();
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        #endregion

        #region GetMovie()

        [Fact]
        public void GetMovie_ValidId_ReturnsMovie()
        {
            SeedData();

            var movie = _service.GetMovie(10);

            Assert.NotNull(movie);
            Assert.Equal(10, movie.Id);
            Assert.Contains(movie.MovieGenres, mg => mg.GenreId == 1);
        }

        [Fact]
        public void GetMovie_IdLessOrEqualZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMovie(0));
        }

        [Fact]
        public void GetMovie_NotFound_ThrowsKeyNotFoundException()
        {
            SeedData();
            Assert.Throws<KeyNotFoundException>(() => _service.GetMovie(999));
        }

        #endregion

        #region AddMovie()

        [Fact]
        public void AddMovie_ValidMovie_Persists()
        {
            var m = new Movie
            {
                Title = "Gamma",
                Description = "Gamma desc",
                Duration = 110,
                ReleaseDate = new DateTime(2025, 1, 1),
                Language = "en",
                Country = "US",
                PosterPath = "/g.jpg"
            };

            _service.AddMovie(m);

            Assert.Equal(1, _context.Movie.Count());
            var saved = _context.Movie.Single();
            Assert.Equal("Gamma", saved.Title);
        }

        [Fact]
        public void AddMovie_NullMovie_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddMovie(null));
        }

        #endregion

        #region RemoveMovie()

        [Fact]
        public void RemoveMovie_ValidId_RemovesMovie()
        {
            SeedData();

            _service.RemoveMovie(10);

            Assert.False(_context.Movie.Any(m => m.Id == 10));
            Assert.False(_context.MovieGenre.Any(mg => mg.MovieId == 10));
        }

        [Fact]
        public void RemoveMovie_IdLessOrEqualZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.RemoveMovie(0));
        }

        [Fact]
        public void RemoveMovie_NotFound_ThrowsKeyNotFoundException()
        {
            SeedData();
            Assert.Throws<KeyNotFoundException>(() => _service.RemoveMovie(999));
        }

        #endregion

        #region SyncMovies()

        [Fact]
        public async Task SyncMovies_UpsertsTrending_RemovesNonTrending_AndSyncsGenres()
        {
            SeedData();

            _tmdbMock
                .Setup(x => x.GetTrendingMoviesWithDetails("week"))
                .ReturnsAsync(new List<MovieFromAPI>
                {
            new MovieFromAPI
            {
                Id = 10,
                Title = "Alpha UPDATED",
                Overview = "Updated overview",
                Runtime = 130,
                OriginalLanguage = "en",
                ReleaseDate = "2024-01-01",
                OriginCountry = new() { "US" },
                PosterPath = "/a2.jpg",
                Genres = new List<Genre>
                {
                    new Genre { Id = 3, Name = "Thriller" }
                }
            },
            new MovieFromAPI
            {
                Id = 12,
                Title = "Newbie",
                Overview = "Brand new",
                Runtime = 100,
                OriginalLanguage = "es",
                ReleaseDate = "2024-10-10",
                OriginCountry = new() { "ES" },
                PosterPath = "/n.jpg",
                Genres = new List<Genre>
                {
                    new Genre { Id = 1, Name = "Action" }
                }
            }
                });

            await _service.SyncMovies();

            Assert.False(_context.Movie.Any(m => m.Id == 11));
            Assert.False(_context.MovieGenre.Any(mg => mg.MovieId == 11));

            var m10 = _context.Movie
                .Include(x => x.MovieGenres)
                .First(m => m.Id == 10);

            Assert.Equal("Alpha UPDATED", m10.Title);
            Assert.Equal("Updated overview", m10.Description);
            Assert.Equal(130, m10.Duration);
            Assert.Equal("/a2.jpg", m10.PosterPath);
            Assert.Single(m10.MovieGenres);
            Assert.Equal(3, m10.MovieGenres.First().GenreId);

            var m12 = _context.Movie
                .Include(x => x.MovieGenres)
                .First(m => m.Id == 12);

            Assert.Equal("Newbie", m12.Title);
            Assert.Equal("es", m12.Language);
            Assert.Equal("ES", m12.Country);
            Assert.Single(m12.MovieGenres);
            Assert.Equal(1, m12.MovieGenres.First().GenreId);
        }

        [Fact]
        public async Task SyncMovies_WhenTrendingEmpty_RemovesAllExisting()
        {
            SeedData();

            _tmdbMock.Setup(x => x.GetTrendingMoviesWithDetails("week"))
                     .ReturnsAsync(new List<MovieFromAPI>());

            await _service.SyncMovies();

            Assert.Empty(_context.Movie.ToList());
            Assert.Empty(_context.MovieGenre.ToList());
        }

        [Fact]
        public async Task SyncMovies_WhenNothingChanged_DoesNotUpdateEntityOrGenres()
        {
            SeedData();

            var beforeUpdated = _context.Movie.First(m => m.Id == 10).UpdatedAt;
            var beforeLinks = _context.MovieGenre.Count(mg => mg.MovieId == 10);

            _tmdbMock.Setup(x => x.GetTrendingMoviesWithDetails("week"))
                     .ReturnsAsync(new List<MovieFromAPI>
                     {
                 new MovieFromAPI
                 {
                     Id = 10,
                     Title = "Alpha",
                     Overview = "Alpha desc",
                     Runtime = 120,
                     OriginalLanguage = "en",
                     ReleaseDate = "2024-01-01",
                     OriginCountry = new() { "US" },
                     PosterPath = "/a.jpg",
                     Genres = new List<Genre> { new Genre { Id = 1, Name = "Action" } }
                 }
                     });

            await Task.Delay(20);

            await _service.SyncMovies();

            var after = _context.Movie.Include(m => m.MovieGenres).First(m => m.Id == 10);
            var afterLinks = _context.MovieGenre.Count(mg => mg.MovieId == 10);

            Assert.Equal(beforeUpdated, after.UpdatedAt);
            Assert.Equal(beforeLinks, afterLinks);
            Assert.Single(after.MovieGenres);
            Assert.Equal(1, after.MovieGenres.First().GenreId);
        }

        [Fact]
        public async Task SyncMovies_RuntimeZero_DoesNotChangeDuration()
        {
            SeedData();
            var original = _context.Movie.First(m => m.Id == 10).Duration;

            _tmdbMock.Setup(x => x.GetTrendingMoviesWithDetails("week"))
                     .ReturnsAsync(new List<MovieFromAPI>
                     {
                 new MovieFromAPI
                 {
                     Id = 10,
                     Title = "Alpha",
                     Overview = "Alpha desc",
                     Runtime = 0,
                     OriginalLanguage = "en",
                     ReleaseDate = "2024-01-01",
                     OriginCountry = new() { "US" },
                     PosterPath = "/a.jpg",
                     Genres = new List<Genre> { new Genre { Id = 1, Name = "Action" } }
                 }
                     });

            await _service.SyncMovies();

            var after = _context.Movie.First(m => m.Id == 10);
            Assert.Equal(original, after.Duration);
        }

        [Fact]
        public async Task SyncMovies_InvalidReleaseDate_UsesUtcNowDate()
        {
            SeedData();
            var oldDate = _context.Movie.First(m => m.Id == 10).ReleaseDate;

            _tmdbMock.Setup(x => x.GetTrendingMoviesWithDetails("week"))
                     .ReturnsAsync(new List<MovieFromAPI>
                     {
                 new MovieFromAPI
                 {
                     Id = 10,
                     Title = "Alpha",
                     Overview = "Alpha desc",
                     Runtime = 120,
                     OriginalLanguage = "en",
                     ReleaseDate = "not-a-date",
                     OriginCountry = new() { "US" },
                     PosterPath = "/a.jpg",
                     Genres = new List<Genre> { new Genre { Id = 1, Name = "Action" } }
                 }
                     });

            await _service.SyncMovies();

            var after = _context.Movie.First(m => m.Id == 10);
            Assert.NotEqual(oldDate, after.ReleaseDate);
            Assert.Equal(DateTime.UtcNow.Date, after.ReleaseDate);
        }

        [Fact]
        public async Task SyncMovies_GenresAlreadyMatch_NoRebuildRelations()
        {
            SeedData();

            var beforeLinks = _context.MovieGenre
                .Where(mg => mg.MovieId == 10).Select(mg => mg.GenreId).ToList();

            _tmdbMock.Setup(x => x.GetTrendingMoviesWithDetails("week"))
                     .ReturnsAsync(new List<MovieFromAPI>
                     {
                 new MovieFromAPI
                 {
                     Id = 10,
                     Title = "Alpha",
                     Overview = "Alpha desc",
                     Runtime = 120,
                     OriginalLanguage = "en",
                     ReleaseDate = "2024-01-01",
                     OriginCountry = new() { "US" },
                     PosterPath = "/a.jpg",
                     Genres = new List<Genre> { new Genre { Id = 1, Name = "Action" } }
                 }
                     });

            await _service.SyncMovies();

            var afterLinks = _context.MovieGenre
                .Where(mg => mg.MovieId == 10).Select(mg => mg.GenreId).ToList();

            Assert.Equal(beforeLinks, afterLinks);
        }

        #endregion
    }
}
