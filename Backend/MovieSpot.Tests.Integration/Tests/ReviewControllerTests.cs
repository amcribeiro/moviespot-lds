using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.Net;
using System.Net.Http.Json;
using static MovieSpot.DTO_s.ReviewDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class ReviewControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;

        public ReviewControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /Review
        [Fact]
        public async Task GetAll_ReturnsOk_WhenReviewsExist()
        {
            await _dataFactory.CreateTestReviewAsync();

            var response = await _client.GetAsync("/Review");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var reviews = await response.Content.ReadFromJsonAsync<IEnumerable<ReviewResponseDto>>();
            Assert.NotEmpty(reviews);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoReviewsExist()
        {
            var response = await _client.GetAsync("/Review");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /Review/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenReviewExists()
        {
            var review = await _dataFactory.CreateTestReviewAsync();

            var response = await _client.GetAsync($"/Review/{review.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ReviewResponseDto>();
            Assert.Equal(review.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenReviewDoesNotExist()
        {
            var response = await _client.GetAsync("/Review/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenIdInvalid()
        {
            var response = await _client.GetAsync("/Review/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region GET /Review/user/{userId}
        [Fact]
        public async Task GetByUser_ReturnsOk_WhenUserHasReviews()
        {
            var review = await _dataFactory.CreateTestReviewAsync();
            var userId = await _dataFactory.GetUserIdByBookingIdAsync(review.BookingId);

            var response = await _client.GetAsync($"/Review/user/{userId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ReviewResponseDto>>();
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetByUser_ReturnsNotFound_WhenUserHasNoReviews()
        {
            var response = await _client.GetAsync("/Review/user/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByUser_ReturnsBadRequest_WhenUserIdInvalid()
        {
            var response = await _client.GetAsync("/Review/user/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region POST /Review
        [Fact]
        public async Task Create_ReturnsOk_WhenValidReview()
        {
            var booking = await _dataFactory.CreateTestBookingAsync();

            var dto = new ReviewCreateDto
            {
                BookingId = booking.Id,
                Rating = 5,
                Comment = "Excelente sessão!"
            };

            var response = await _client.PostAsJsonAsync("/Review", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ReviewResponseDto>();
            Assert.Equal(dto.Rating, created.Rating);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenReviewIsNull()
        {
            var response = await _client.PostAsJsonAsync("/Review", (ReviewCreateDto?)null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /Review/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValidReview()
        {
            var review = await _dataFactory.CreateTestReviewAsync();

            var dto = new ReviewUpdateDto
            {
                Rating = 3,
                Comment = "Foi razoável.",
                ReviewDate = DateTime.UtcNow
            };

            var response = await _client.PutAsJsonAsync($"/Review/{review.Id}", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<ReviewResponseDto>();
            Assert.Equal(3, updated.Rating);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenDtoIsNull()
        {
            var response = await _client.PutAsJsonAsync<ReviewUpdateDto?>("/Review/1", null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenReviewDoesNotExist()
        {
            var dto = new ReviewUpdateDto
            {
                Rating = 2,
                Comment = "Inexistente"
            };

            var response = await _client.PutAsJsonAsync("/Review/9999", dto);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}
