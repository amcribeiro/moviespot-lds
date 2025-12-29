using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using static MovieSpot.DTO_s.SeatDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class SeatControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public SeatControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Seat
        [Fact]
        public async Task GetAll_ReturnsOk_WhenSeatsExist()
        {
            await _dataFactory.CreateTestSeatAsync();

            var response = await _client.GetAsync("/Seat");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var seats = await response.Content.ReadFromJsonAsync<IEnumerable<SeatResponseDto>>();
            Assert.NotEmpty(seats);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoSeatsExist()
        {
            var response = await _client.GetAsync("/Seat");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Seat/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenSeatExists()
        {
            var seat = await _dataFactory.CreateTestSeatAsync();

            var response = await _client.GetAsync($"/Seat/{seat.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SeatResponseDto>();
            Assert.Equal(seat.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenSeatDoesNotExist()
        {
            var response = await _client.GetAsync("/Seat/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Seat/hall/{cinemaHallId}
        [Fact]
        public async Task GetByCinemaHall_ReturnsOk_WhenSeatsExist()
        {
            var seat = await _dataFactory.CreateTestSeatAsync();

            var response = await _client.GetAsync($"/Seat/hall/{seat.CinemaHallId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<SeatResponseDto>>();
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetByCinemaHall_ReturnsNotFound_WhenNoSeatsExist()
        {
            var response = await _client.GetAsync("/Seat/hall/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region POST /Seat
        [Fact]
        public async Task Create_ReturnsCreated_WhenValidSeat()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();

            var dto = new SeatCreateDto
            {
                CinemaHallId = hall.Id,
                SeatNumber = "A1",
                SeatType = "Normal"
            };

            var response = await _client.PostAsJsonAsync("/Seat", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<SeatResponseDto>();
            Assert.Equal(dto.SeatNumber, created.SeatNumber);
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenDuplicateSeatNumber()
        {
            var seat = await _dataFactory.CreateTestSeatAsync();

            var dto = new SeatCreateDto
            {
                CinemaHallId = seat.CinemaHallId,
                SeatNumber = seat.SeatNumber,
                SeatType = "VIP"
            };

            var response = await _client.PostAsJsonAsync("/Seat", dto);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenInvalidSeat()
        {
            var dto = new SeatCreateDto
            {
                CinemaHallId = 0,
                SeatNumber = "",
                SeatType = ""
            };

            var response = await _client.PostAsJsonAsync("/Seat", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /Seat
        [Fact]
        public async Task Update_ReturnsOk_WhenValid()
        {
            var seat = await _dataFactory.CreateTestSeatAsync();

            var dto = new SeatUpdateDto
            {
                Id = seat.Id,
                CinemaHallId = seat.CinemaHallId,
                SeatNumber = "B2",
                SeatType = "VIP"
            };

            var response = await _client.PutAsJsonAsync("/Seat", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<SeatResponseDto>();
            Assert.Equal("B2", updated.SeatNumber);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenSeatDoesNotExist()
        {
            var dto = new SeatUpdateDto
            {
                Id = 9999,
                CinemaHallId = 1,
                SeatNumber = "C1",
                SeatType = "Normal"
            };

            var response = await _client.PutAsJsonAsync("/Seat", dto);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region DELETE /Seat/{id}
        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSeatDeleted()
        {
            var seat = await _dataFactory.CreateTestSeatAsync();

            var response = await _client.DeleteAsync($"/Seat/{seat.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenSeatDoesNotExist()
        {
            var response = await _client.DeleteAsync("/Seat/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}
