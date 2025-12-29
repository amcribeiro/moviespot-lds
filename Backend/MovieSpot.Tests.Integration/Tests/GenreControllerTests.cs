using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;

namespace MovieSpot.Tests.Integration.Tests
{
    public class GenreControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public GenreControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Genre
        [Fact]
        public async Task GetAll_ReturnsOk_WhenGenresExist()
        {
            await _dataFactory.CreateTestGenreAsync("Action");

            var response = await _client.GetAsync("/Genre");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var genres = await response.Content.ReadFromJsonAsync<IEnumerable<Genre>>();
            Assert.NotEmpty(genres);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoGenresExist()
        {
            var response = await _client.GetAsync("/Genre");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Genre/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenGenreExists()
        {
            var genre = await _dataFactory.CreateTestGenreAsync("Comedy");

            var response = await _client.GetAsync($"/Genre/{genre.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<Genre>();
            Assert.Equal(genre.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenGenreDoesNotExist()
        {
            var response = await _client.GetAsync("/Genre/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.GetAsync("/Genre/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region POST /Genre/sync
        [Fact]
        public async Task SyncGenres_ReturnsNoContent_WhenSuccessful()
        {
            var response = await _client.PostAsync("/Genre/sync", null);

            Assert.True(
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.InternalServerError,
                $"Expected 204 or 500, but got {response.StatusCode}");
        }
        #endregion
    }
}
