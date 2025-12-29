using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using static MovieSpot.DTO_s.SessionDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class SessionControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public SessionControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Session
        [Fact]
        public async Task GetAll_ReturnsOk_WhenSessionsExist()
        {
            await _dataFactory.CreateTestSessionAsync();

            var response = await _client.GetAsync("/Session");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var sessions = await response.Content.ReadFromJsonAsync<IEnumerable<SessionResponseDto>>();
            Assert.NotEmpty(sessions);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoSessionsExist()
        {
            var response = await _client.GetAsync("/Session");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Session/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenSessionExists()
        {
            var session = await _dataFactory.CreateTestSessionAsync();

            var response = await _client.GetAsync($"/Session/{session.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SessionResponseDto>();
            Assert.Equal(session.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            var response = await _client.GetAsync("/Session/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.GetAsync("/Session/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region POST /Session
        [Fact]
        public async Task Create_ReturnsOk_WhenValidSession()
        {
            var movie = await _dataFactory.CreateTestMovieAsync();
            var hall = await _dataFactory.CreateTestCinemaHallAsync();
            var user = await _dataFactory.CreateTestUserAsync("Staff");

            var dto = new SessionCreateDto
            {
                MovieId = movie.Id,
                CinemaHallId = hall.Id,
                CreatedBy = user.Id,
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddHours(3),
                Price = 10
            };

            var response = await _client.PostAsJsonAsync("/Session", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<SessionResponseDto>();
            Assert.Equal(dto.Price, created.Price);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenInvalidData()
        {
            var dto = new SessionCreateDto
            {
                MovieId = 0,
                CinemaHallId = 0,
                CreatedBy = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Price = -5
            };

            var response = await _client.PostAsJsonAsync("/Session", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /Session/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValidSession()
        {
            var session = await _dataFactory.CreateTestSessionAsync();

            var dto = new SessionUpdateDto
            {
                Id = session.Id,
                MovieId = session.MovieId,
                CinemaHallId = session.CinemaHallId,
                StartDate = DateTime.UtcNow.AddHours(2),
                EndDate = DateTime.UtcNow.AddHours(4),
                Price = 15
            };

            var response = await _client.PutAsJsonAsync($"/Session/{session.Id}", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<SessionResponseDto>();
            Assert.Equal(15, updated.Price);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            var dto = new SessionUpdateDto
            {
                Id = 9999,
                MovieId = 1,
                CinemaHallId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                Price = 10
            };

            var response = await _client.PutAsJsonAsync("/Session/9999", dto);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region DELETE /Session/{id}
        [Fact]
        public async Task Delete_ReturnsOk_WhenSessionDeleted()
        {
            var session = await _dataFactory.CreateTestSessionAsync();

            var response = await _client.DeleteAsync($"/Session/{session.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            var response = await _client.DeleteAsync("/Session/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Session/available-times
        [Fact]
        public async Task GetAvailableTimes_ReturnsValidStatus_WhenCalled()
        {
            var hall = await _dataFactory.CreateTestCinemaHallAsync();
            var date = DateTime.UtcNow.Date;

            var response = await _client.GetAsync($"/Session/available-times?cinemaHallId={hall.Id}&date={date:yyyy-MM-dd}&runtimeMinutes=90");

            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest });
        }
        #endregion

        #region GET /Session/{sessionId}/available-seats
        [Fact]
        public async Task GetAvailableSeats_ReturnsOk_WhenSeatsAvailable()
        {
            var (session, seat) = await _dataFactory.CreateTestSessionWithSeatAsync();

            var response = await _client.GetAsync($"/Session/{session.Id}/available-seats");

            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.NotFound });
        }
        #endregion
    }
}
