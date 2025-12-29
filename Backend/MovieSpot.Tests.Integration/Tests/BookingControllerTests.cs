using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using static MovieSpot.DTO_s.BookingDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class BookingControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public BookingControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Booking
        [Fact]
        public async Task GetAll_ReturnsOk_WhenBookingsExist()
        {
            await _dataFactory.CreateTestBookingAsync();

            var response = await _client.GetAsync("/Booking");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var bookings = await response.Content.ReadFromJsonAsync<IEnumerable<BookingResponseDto>>();
            Assert.NotEmpty(bookings);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoBookingsExist()
        {
            var response = await _client.GetAsync("/Booking");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Booking/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenBookingExists()
        {
            var booking = await _dataFactory.CreateTestBookingAsync();

            var response = await _client.GetAsync($"/Booking/{booking.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
            Assert.Equal(booking.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenBookingDoesNotExist()
        {
            var response = await _client.GetAsync("/Booking/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Booking/user/{userId}
        [Fact]
        public async Task GetByUser_ReturnsOk_WhenUserHasBookings()
        {
            var booking = await _dataFactory.CreateTestBookingAsync();

            var response = await _client.GetAsync($"/Booking/user/{booking.UserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<BookingResponseDto>>();
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetByUser_ReturnsNotFound_WhenUserHasNoBookings()
        {
            var response = await _client.GetAsync("/Booking/user/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region POST /Booking
        [Fact]
        public async Task Create_ReturnsOk_WhenValidBooking()
        {
            var user = await _dataFactory.CreateTestUserAsync();
            var session = await _dataFactory.CreateTestSessionAsync(user.Id);
            var seat = await _dataFactory.CreateTestSeatAsync();

            var dto = new BookingCreateDto
            {
                UserId = user.Id,
                SessionId = session.Id,
                SeatIds = new List<int> { seat.Id }
            };

            var response = await _client.PostAsJsonAsync("/Booking", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
            Assert.Equal(user.Id, created.UserId);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenBookingIsNull()
        {
            var response = await _client.PostAsJsonAsync("/Booking", (BookingCreateDto?)null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /Booking/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValidBooking()
        {
            var booking = await _dataFactory.CreateTestBookingAsync();

            var updateDto = new BookingUpdateDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                SessionId = booking.SessionId,
                Status = false,
                TotalAmount = 30
            };

            var response = await _client.PutAsJsonAsync($"/Booking/{booking.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
            Assert.False(updated.Status);
            Assert.Equal(30, updated.TotalAmount);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var booking = await _dataFactory.CreateTestBookingAsync();

            var updateDto = new BookingUpdateDto
            {
                Id = booking.Id + 1,
                UserId = booking.UserId,
                SessionId = booking.SessionId,
                Status = false,
                TotalAmount = 30
            };

            var response = await _client.PutAsJsonAsync($"/Booking/{booking.Id}", updateDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
    }
}
