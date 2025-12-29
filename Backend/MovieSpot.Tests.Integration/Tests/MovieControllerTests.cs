using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;

namespace MovieSpot.Tests.Integration.Tests
{
    public class MovieControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public MovieControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Movie
        [Fact]
        public async Task GetAll_ReturnsOk_WhenMoviesExist()
        {
            await _dataFactory.CreateTestMovieAsync();

            var response = await _client.GetAsync("/Movie");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var movies = await response.Content.ReadFromJsonAsync<IEnumerable<MovieDto>>();
            Assert.NotEmpty(movies);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoMoviesExist()
        {
            var response = await _client.GetAsync("/Movie");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Movie/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenMovieExists()
        {
            var movie = await _dataFactory.CreateTestMovieAsync();

            var response = await _client.GetAsync($"/Movie/{movie.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MovieDto>();
            Assert.Equal(movie.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMovieDoesNotExist()
        {
            var response = await _client.GetAsync("/Movie/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.GetAsync("/Movie/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region POST /Movie/add
        [Fact]
        public async Task AddMovie_ReturnsCreated_WhenValid()
        {
            var movie = new Movie
            {
                Title = "Inception",
                Description = "Dreams within dreams.",
                Duration = 148,
                ReleaseDate = DateTime.UtcNow.AddYears(-10),
                Language = "English",
                Country = "USA",
                PosterPath = "/poster.jpg"
            };

            var response = await _client.PostAsJsonAsync("/Movie/add", movie);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AddMovie_ReturnsBadRequest_WhenMovieIsNull()
        {
            var response = await _client.PostAsJsonAsync<Movie?>("/Movie/add", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region DELETE /Movie/remove/{id}
        [Fact]
        public async Task RemoveMovie_ReturnsNoContent_WhenMovieExists()
        {
            var movie = await _dataFactory.CreateTestMovieAsync();

            var response = await _client.DeleteAsync($"/Movie/remove/{movie.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task RemoveMovie_ReturnsNotFound_WhenMovieDoesNotExist()
        {
            var response = await _client.DeleteAsync("/Movie/remove/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RemoveMovie_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync("/Movie/remove/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
    }
}