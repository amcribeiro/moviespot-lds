using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using static MovieSpot.DTO_s.CinemaHallDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class CinemaHallControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public CinemaHallControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /CinemaHall
        [Fact]
        public async Task GetAll_ReturnsOk_WhenCinemaHallsExist()
        {
            await _dataFactory.CreateTestCinemaHallAsync();

            var response = await _client.GetAsync("/CinemaHall");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var halls = await response.Content.ReadFromJsonAsync<IEnumerable<CinemaHallReadDto>>();
            Assert.NotEmpty(halls);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoCinemaHallsExist()
        {
            var response = await _client.GetAsync("/CinemaHall");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /CinemaHall/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenCinemaHallExists()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();

            var response = await _client.GetAsync($"/CinemaHall/{hall.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CinemaHallDetailsDto>();
            Assert.Equal(hall.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCinemaHallDoesNotExist()
        {
            var response = await _client.GetAsync("/CinemaHall/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /CinemaHall/cinema/{cinemaId}
        [Fact]
        public async Task GetByCinemaId_ReturnsOk_WhenCinemaHasHalls()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();

            var response = await _client.GetAsync($"/CinemaHall/cinema/{hall.CinemaId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var halls = await response.Content.ReadFromJsonAsync<IEnumerable<CinemaHallReadDto>>();
            Assert.NotEmpty(halls);
        }

        [Fact]
        public async Task GetByCinemaId_ReturnsNotFound_WhenNoHallsForCinema()
        {
            var cinema = await _dataFactory.CreateTestCinemaAsync();

            var response = await _client.GetAsync($"/CinemaHall/cinema/{cinema.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region POST /CinemaHall
        [Fact]
        public async Task Create_ReturnsOk_WhenValidData()
        {
            var cinema = await _dataFactory.CreateTestCinemaAsync();

            var dto = new CinemaHallCreateDto
            {
                Name = "Sala Principal",
                CinemaId = cinema.Id
            };

            var response = await _client.PostAsJsonAsync("/CinemaHall", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<CinemaHallReadDto>();
            Assert.Equal(dto.Name, created.Name);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenMissingData()
        {
            var dto = new CinemaHallCreateDto
            {
                Name = "",
                CinemaId = 0
            };

            var response = await _client.PostAsJsonAsync("/CinemaHall", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /CinemaHall/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValid()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();

            var dto = new CinemaHallUpdateDto
            {
                Id = hall.Id,
                Name = "Sala Atualizada",
                CinemaId = hall.CinemaId
            };

            var response = await _client.PutAsJsonAsync($"/CinemaHall/{hall.Id}", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<CinemaHallReadDto>();
            Assert.Equal("Sala Atualizada", updated.Name);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();

            var dto = new CinemaHallUpdateDto
            {
                Id = hall.Id + 1,
                Name = "Sala Errada",
                CinemaId = hall.CinemaId
            };

            var response = await _client.PutAsJsonAsync($"/CinemaHall/{hall.Id}", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region DELETE /CinemaHall/{id}
        [Fact]
        public async Task Delete_ReturnsOk_WhenCinemaHallExists()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();

            var response = await _client.DeleteAsync($"/CinemaHall/{hall.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCinemaHallDoesNotExist()
        {
            var response = await _client.DeleteAsync("/CinemaHall/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}
