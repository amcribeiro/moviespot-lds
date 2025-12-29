using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Genres;
using Newtonsoft.Json;

namespace MovieSpot.Tests.Services.Genres
{
    public class GenreServiceTest
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly ApplicationDbContext _context;
        private readonly GenreService _service;

        public GenreServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["TMDB:ApiKey"]).Returns("fake_key");
            _mockConfig.Setup(c => c["TMDB:BaseUrl"]).Returns("https://fakeapi.com");

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _service = new GenreService(_context, _mockHttpClientFactory.Object, _mockConfig.Object);
        }

        #region Helpers

        private void CreateMockHttpClient(HttpStatusCode statusCode, string jsonResponse)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(jsonResponse)
                });

            var client = new HttpClient(handlerMock.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        }

        private void CreateThrowingHttpClient(Exception ex)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(ex);

            var client = new HttpClient(handlerMock.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        }

        #endregion

        #region GetAllGenresFromApiAsync

        [Fact]
        public async Task GetAllGenresFromApiAsync_ReturnsGenres_OnSuccess()
        {
            var json = "{\"genres\": [{\"id\": 1, \"name\": \"Action\"}, {\"id\": 2, \"name\": \"Drama\"}]}";
            CreateMockHttpClient(HttpStatusCode.OK, json);

            var result = await _service.GetAllGenresFromApiAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Action", result[0].Name);
        }

        [Fact]
        public async Task GetAllGenresFromApiAsync_MapsUnknownName_WhenApiNameNull()
        {
            var json = "{\"genres\": [{\"id\": 7, \"name\": null}]}";
            CreateMockHttpClient(HttpStatusCode.OK, json);

            var result = await _service.GetAllGenresFromApiAsync();

            Assert.Single(result);
            Assert.Equal(7, result[0].Id);
            Assert.Equal("Unknown", result[0].Name);
        }

        [Fact]
        public async Task GetAllGenresFromApiAsync_MissingGenresKey_ThrowsInvalidOperationException()
        {
            var json = "{\"somethingElse\": []}";
            CreateMockHttpClient(HttpStatusCode.OK, json);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllGenresFromApiAsync());
        }

        [Fact]
        public async Task GetAllGenresFromApiAsync_NonSuccessStatus_ThrowsHttpRequestException()
        {
            CreateMockHttpClient(HttpStatusCode.BadGateway, "{}");

            await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetAllGenresFromApiAsync());
        }

        [Fact]
        public async Task GetAllGenresFromApiAsync_HandlerThrows_PropagatesHttpRequestException()
        {
            CreateThrowingHttpClient(new HttpRequestException("network down"));

            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetAllGenresFromApiAsync());
            Assert.Contains("network down", ex.Message);
        }

        [Fact]
        public async Task GetAllGenresFromApiAsync_InvalidJson_ThrowsJsonReaderException()
        {
            var invalidJson = "{ invalid json ";
            CreateMockHttpClient(HttpStatusCode.OK, invalidJson);

            await Assert.ThrowsAsync<JsonReaderException>(() => _service.GetAllGenresFromApiAsync());
        }

        #endregion

        #region GetAllGenresFromDb

        [Fact]
        public void GetAllGenresFromDb_ReturnsList_WhenDataExists()
        {
            _context.Genre.AddRange(
                new Genre { Id = 1, Name = "Action" },
                new Genre { Id = 2, Name = "Comedy" }
            );
            _context.SaveChanges();

            var result = _service.GetAllGenresFromDb();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Name == "Comedy");
        }

        [Fact]
        public void GetAllGenresFromDb_Empty_ThrowsInvalidOperationException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var freshContext = new ApplicationDbContext(options);
            var service = new GenreService(freshContext, _mockHttpClientFactory.Object, _mockConfig.Object);

            Assert.Throws<InvalidOperationException>(() => service.GetAllGenresFromDb());
        }

        #endregion

        #region GetGenreById

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetGenreById_IdLessOrEqualZero_ThrowsArgumentOutOfRangeException(int badId)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetGenreById(badId));
        }

        [Fact]
        public void GetGenreById_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetGenreById(999));
        }

        [Fact]
        public void GetGenreById_ReturnsGenre_WhenExists()
        {
            _context.Genre.Add(new Genre { Id = 10, Name = "Sci-Fi" });
            _context.SaveChanges();

            var genre = _service.GetGenreById(10);

            Assert.NotNull(genre);
            Assert.Equal("Sci-Fi", genre.Name);
        }

        #endregion

        #region SyncGenresAsync

        [Fact]
        public async Task SyncGenresAsync_AddsNew_OnEmptyDb()
        {
            var json = "{\"genres\": [{\"id\": 1, \"name\": \"Action\"}, {\"id\": 2, \"name\": \"Drama\"}]}";
            CreateMockHttpClient(HttpStatusCode.OK, json);

            await _service.SyncGenresAsync();

            var all = _context.Genre.ToList();
            Assert.Equal(2, all.Count);
            Assert.Contains(all, g => g.Id == 1 && g.Name == "Action");
        }

        [Fact]
        public async Task SyncGenresAsync_UpdatesExistingName_WhenChanged()
        {
            _context.Genre.Add(new Genre { Id = 1, Name = "Old" });
            _context.SaveChanges();

            var json = "{\"genres\": [{\"id\": 1, \"name\": \"New\"}]}";
            CreateMockHttpClient(HttpStatusCode.OK, json);

            await _service.SyncGenresAsync();

            var updated = _context.Genre.Find(1)!;
            Assert.Equal("New", updated.Name);
        }

        [Fact]
        public async Task SyncGenresAsync_HandlerThrows_PropagatesHttpRequestException()
        {
            CreateThrowingHttpClient(new HttpRequestException("boom"));

            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _service.SyncGenresAsync());
            Assert.Contains("boom", ex.Message);
        }

        [Fact]
        public async Task SyncGenresAsync_InvalidJson_ThrowsWrappedJsonReaderException()
        {
            var invalidJson = "{ bad json ";
            CreateMockHttpClient(HttpStatusCode.OK, invalidJson);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.SyncGenresAsync());
            Assert.Equal("An unexpected error occurred during genre synchronization.", ex.Message);
            Assert.IsType<JsonReaderException>(ex.InnerException);
        }

        [Fact]
        public async Task SyncGenresAsync_DoesNotDuplicateExistingIds()
        {
            _context.Genre.Add(new Genre { Id = 5, Name = "FirstName" });
            _context.SaveChanges();

            var json = "{\"genres\": [{\"id\": 5, \"name\": \"FirstName\"}, {\"id\": 6, \"name\": \"Second\"}]}";
            CreateMockHttpClient(HttpStatusCode.OK, json);

            await _service.SyncGenresAsync();

            var all = _context.Genre.OrderBy(g => g.Id).ToList();
            Assert.Equal(2, all.Count);
            Assert.Equal("FirstName", all[0].Name);
            Assert.Equal("Second", all[1].Name);
        }

        [Fact]
        public async Task SyncGenresAsync_SaveChangesThrows_ThrowsDbUpdateException()
        {
            var okJson = "{\"genres\": [{\"id\": 3, \"name\": \"Thriller\"}]}";
            CreateMockHttpClient(HttpStatusCode.OK, okJson);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingContext(options);
            var service = new GenreService(failingContext, _mockHttpClientFactory.Object, _mockConfig.Object);

            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => service.SyncGenresAsync());
            Assert.Contains("Failed to synchronize genres with the local database.", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.Contains("Save failed", ex.InnerException!.Message);
        }

        #endregion

        #region FailingContext
        private class FailingContext : ApplicationDbContext
        {
            public FailingContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new DbUpdateException("Save failed");
            }
        }

        #endregion
    }
}
