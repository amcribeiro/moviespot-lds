using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using static MovieSpot.DTO_s.CinemaDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class CinemaControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public CinemaControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Cinemas
        [Fact]
        public async Task GetAll_ReturnsOk_WhenCinemasExist()
        {
            await _dataFactory.CreateTestCinemaAsync();

            var response = await _client.GetAsync("/Cinemas");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var cinemas = await response.Content.ReadFromJsonAsync<IEnumerable<CinemaResponseDto>>();
            Assert.NotEmpty(cinemas);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoCinemasExist()
        {
            var response = await _client.GetAsync("/Cinemas");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Cinemas/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenCinemaExists()
        {
            var cinema = await _dataFactory.CreateTestCinemaAsync();

            var response = await _client.GetAsync($"/Cinemas/{cinema.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CinemaResponseDto>();
            Assert.Equal(cinema.Name, result.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCinemaDoesNotExist()
        {
            var response = await _client.GetAsync("/Cinemas/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.GetAsync("/Cinemas/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region POST /Cinemas
        [Fact]
        public async Task Create_ReturnsCreated_WhenValidData()
        {
            var dto = new CinemaCreateDto
            {
                Name = "Cinema Braga",
                Street = "Rua Central",
                City = "Braga",
                State = "Braga",
                ZipCode = "4700-000",
                Country = "Portugal",
                Latitude = 41.55m,
                Longitude = -8.42m
            };

            var response = await _client.PostAsJsonAsync("/Cinemas", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<CinemaResponseDto>();
            Assert.Equal("Cinema Braga", created.Name);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelInvalid()
        {
            var dto = new CinemaCreateDto
            {
                Street = "Rua X",
                Country = "Portugal",
                Latitude = 40.0m,
                Longitude = -8.0m
            };

            var response = await _client.PostAsJsonAsync("/Cinemas", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /Cinemas/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValidData()
        {
            var cinema = await _dataFactory.CreateTestCinemaAsync();

            var updateDto = new CinemaUpdateDto
            {
                Id = cinema.Id,
                Name = "Novo Cinema Coimbra",
                Street = "Rua Nova",
                City = "Coimbra",
                Country = "Portugal",
                Latitude = 40.21m,
                Longitude = -8.41m
            };

            var response = await _client.PutAsJsonAsync($"/Cinemas/{cinema.Id}", updateDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<CinemaResponseDto>();
            Assert.Equal("Novo Cinema Coimbra", updated.Name);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var cinema = await _dataFactory.CreateTestCinemaAsync();

            var updateDto = new CinemaUpdateDto
            {
                Id = cinema.Id + 1,
                Name = "CineX",
                Street = "Rua XPTO",
                City = "Aveiro",
                Country = "Portugal",
                Latitude = 40.6m,
                Longitude = -8.6m
            };

            var response = await _client.PutAsJsonAsync($"/Cinemas/{cinema.Id}", updateDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenCinemaDoesNotExist()
        {
            var updateDto = new CinemaUpdateDto
            {
                Id = 9999,
                Name = "Cine Fantasma",
                Street = "Rua Perdida",
                City = "Lisboa",
                Country = "Portugal",
                Latitude = 38.7m,
                Longitude = -9.1m
            };

            var response = await _client.PutAsJsonAsync("/Cinemas/9999", updateDto);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region DELETE /Cinemas/{id}
        [Fact]
        public async Task Delete_ReturnsNoContent_WhenCinemaExists()
        {
            var cinema = await _dataFactory.CreateTestCinemaAsync();

            var response = await _client.DeleteAsync($"/Cinemas/{cinema.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCinemaDoesNotExist()
        {
            var response = await _client.DeleteAsync("/Cinemas/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync("/Cinemas/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
    }
}
