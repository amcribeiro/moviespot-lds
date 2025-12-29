using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Reviews;
using Xunit;
using static MovieSpot.DTO_s.ReviewDTO;

namespace MovieSpot.Tests.Controllers.Reviews
{
    public class ReviewControllerTest
    {
        private readonly Mock<IReviewService> _mockService;
        private readonly ReviewController _controller;

        public ReviewControllerTest()
        {
            _mockService = new Mock<IReviewService>();
            _controller = new ReviewController(_mockService.Object);
        }

        private Review CreateReview() => new Review
        {
            Id = 1,
            BookingId = 10,
            Rating = 5,
            Comment = "Excelente filme!",
            ReviewDate = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        #region GetAllReviews

        [Fact]
        public void GetAllReviews_ReturnsOk_WhenReviewsExist()
        {
            var reviews = new List<Review> { CreateReview() };
            _mockService.Setup(s => s.GetAllReviews()).Returns(reviews);

            var result = _controller.GetAllReviews() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<ReviewResponseDto>>(result.Value);
            Assert.Single(value);
            Assert.Equal(5, value.First().Rating);
        }

        [Fact]
        public void GetAllReviews_ReturnsNotFound_WhenNoReviews()
        {
            _mockService.Setup(s => s.GetAllReviews())
                .Throws(new InvalidOperationException("There are no reviews registered in the system."));

            var result = _controller.GetAllReviews() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("no reviews registered", result.Value!.ToString().ToLower());
        }

        #endregion

        #region GetReviewById

        [Fact]
        public void GetReviewById_ReturnsOk_WhenValidId()
        {
            var review = CreateReview();
            _mockService.Setup(s => s.GetReviewById(1)).Returns(review);

            var result = _controller.GetReviewById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var dto = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Equal(5, dto.Rating);
            Assert.Equal("Excelente filme!", dto.Comment);
        }

        [Fact]
        public void GetReviewById_ReturnsBadRequest_WhenInvalidId()
        {
            _mockService.Setup(s => s.GetReviewById(0))
                .Throws(new ArgumentOutOfRangeException("id", "The review ID must be greater than zero."));

            var result = _controller.GetReviewById(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater than zero", result.Value!.ToString().ToLower());
        }

        [Fact]
        public void GetReviewById_ReturnsNotFound_WhenKeyNotFound()
        {
            _mockService.Setup(s => s.GetReviewById(1))
                .Throws(new KeyNotFoundException("Review not found."));

            var result = _controller.GetReviewById(1) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("review not found", result.Value!.ToString().ToLower());
        }

        #endregion

        #region GetAllReviewsByUserId

        [Fact]
        public void GetAllReviewsByUserId_ReturnsOk_WhenValidUser()
        {
            var reviews = new List<Review> { CreateReview() };
            _mockService.Setup(s => s.GetAllReviewsByUserId(2)).Returns(reviews);

            var result = _controller.GetAllReviewsByUserId(2) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<ReviewResponseDto>>(result.Value);
            Assert.Single(value);
        }

        [Fact]
        public void GetAllReviewsByUserId_ReturnsBadRequest_WhenInvalidUserId()
        {
            _mockService.Setup(s => s.GetAllReviewsByUserId(0))
                .Throws(new ArgumentOutOfRangeException("userId", "The user ID must be greater than zero."));

            var result = _controller.GetAllReviewsByUserId(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater than zero", result.Value!.ToString().ToLower());
        }

        [Fact]
        public void GetAllReviewsByUserId_ReturnsNotFound_WhenUserHasNoReviews()
        {
            _mockService.Setup(s => s.GetAllReviewsByUserId(3))
                .Throws(new InvalidOperationException("No reviews were found for this user."));

            var result = _controller.GetAllReviewsByUserId(3) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("no reviews were found", result.Value!.ToString().ToLower());
        }

        #endregion

        #region CreateReview

        [Fact]
        public void CreateReview_ReturnsOk_WhenValidRequest()
        {
            var dto = new ReviewCreateDto
            {
                BookingId = 10,
                Rating = 4,
                Comment = "Bom filme."
            };

            var created = CreateReview();
            _mockService.Setup(s => s.CreateReview(It.IsAny<Review>())).Returns(created);

            var result = _controller.Create(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Equal(5, response.Rating);
        }

        [Fact]
        public void CreateReview_ReturnsBadRequest_WhenDtoNull()
        {
            var result = _controller.Create(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Review cannot be null.", result!.Value!.ToString());
        }

        [Fact]
        public void CreateReview_ReturnsBadRequest_WhenInvalidOperation()
        {
            var dto = new ReviewCreateDto { BookingId = 5, Rating = 2 };
            _mockService.Setup(s => s.CreateReview(It.IsAny<Review>()))
                .Throws(new InvalidOperationException("Each booking can have only one review."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("only one review", result!.Value!.ToString().ToLower());
        }

        [Fact]
        public void CreateReview_ReturnsNotFound_WhenBookingDoesNotExist()
        {
            var dto = new ReviewCreateDto { BookingId = 999, Rating = 5 };
            _mockService.Setup(s => s.CreateReview(It.IsAny<Review>()))
                .Throws(new KeyNotFoundException("Booking not found."));

            var result = _controller.Create(dto) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("booking not found", result.Value!.ToString().ToLower());
        }

        #endregion

        #region UpdateReview

        [Fact]
        public void UpdateReview_ReturnsOk_WhenValidRequest()
        {
            var dto = new ReviewUpdateDto
            {
                Id = 1,
                Rating = 3,
                Comment = "Atualizada"
            };

            var updated = CreateReview();
            _mockService.Setup(s => s.UpdateReview(1, It.IsAny<Review>())).Returns(updated);

            var result = _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Equal(5, response.Rating);
        }

        [Fact]
        public void UpdateReview_ReturnsBadRequest_WhenDtoNull()
        {
            var result = _controller.Update(1, null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Review cannot be null.", result!.Value!.ToString());
        }

        [Fact]
        public void UpdateReview_ReturnsNotFound_WhenReviewNotExists()
        {
            var dto = new ReviewUpdateDto { Id = 1, Rating = 2 };

            _mockService.Setup(s => s.UpdateReview(1, It.IsAny<Review>()))
                .Throws(new KeyNotFoundException("Review not found."));

            var result = _controller.Update(1, dto) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result!.StatusCode);
            Assert.Contains("Review not found.", result.Value!.ToString());
        }

        [Fact]
        public void UpdateReview_ReturnsBadRequest_WhenInvalidOperation()
        {
            var dto = new ReviewUpdateDto { Id = 1, Rating = 2 };
            _mockService.Setup(s => s.UpdateReview(1, It.IsAny<Review>()))
                .Throws(new InvalidOperationException("It is not allowed to change the booking associated with the review."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("not allowed to change", result.Value!.ToString().ToLower());
        }

        #endregion

        #region CreateReview Exception Coverage

        [Fact]
        public void CreateReview_ReturnsBadRequest_WhenArgumentNullExceptionThrown()
        {
            var dto = new ReviewCreateDto
            {
                BookingId = 1,
                Rating = 5,
                Comment = "Teste"
            };

            _mockService.Setup(s => s.CreateReview(It.IsAny<Review>()))
                .Throws(new ArgumentNullException("review", "The provided review is null."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("review is null", result.Value!.ToString().ToLower());
        }

        [Fact]
        public void CreateReview_ReturnsBadRequest_WhenDbUpdateExceptionThrown()
        {
            var dto = new ReviewCreateDto
            {
                BookingId = 1,
                Rating = 4,
                Comment = "Falha no insert"
            };

            _mockService.Setup(s => s.CreateReview(It.IsAny<Review>()))
                .Throws(new DbUpdateException("Database error while creating review."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("database error", result.Value!.ToString().ToLower());
        }

        #endregion

        #region UpdateReview Exception Coverage

        [Fact]
        public void UpdateReview_ReturnsBadRequest_WhenArgumentNullExceptionThrown()
        {
            var dto = new ReviewUpdateDto
            {
                Id = 1,
                Rating = 3,
                Comment = "Erro de null"
            };

            _mockService.Setup(s => s.UpdateReview(1, It.IsAny<Review>()))
                .Throws(new ArgumentNullException("review", "The review object is null."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("review object is null", result.Value!.ToString().ToLower());
        }

        [Fact]
        public void UpdateReview_ReturnsBadRequest_WhenArgumentOutOfRangeExceptionThrown()
        {
            var dto = new ReviewUpdateDto
            {
                Id = -1,
                Rating = 4,
                Comment = "Valor fora de intervalo"
            };

            _mockService.Setup(s => s.UpdateReview(-1, It.IsAny<Review>()))
                .Throws(new ArgumentOutOfRangeException("id", "The provided ID is outside the valid range."));

            var result = _controller.Update(-1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("outside the valid range", result.Value!.ToString().ToLower());
        }

        [Fact]
        public void UpdateReview_ReturnsBadRequest_WhenDbUpdateExceptionThrown()
        {
            var dto = new ReviewUpdateDto
            {
                Id = 1,
                Rating = 2,
                Comment = "Falha no update"
            };

            _mockService.Setup(s => s.UpdateReview(1, It.IsAny<Review>()))
                .Throws(new DbUpdateException("Database error while updating review."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("database error", result.Value!.ToString().ToLower());
        }

        #endregion

        #region Null Paths & Date Coverage

        [Fact]
        public void GetReviewById_ReturnsOk_WhenBookingIsNull()
        {
            var review = new Review
            {
                Id = 1,
                Rating = 5,
                Comment = "Sem booking",
                ReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Booking = null
            };

            _mockService.Setup(s => s.GetReviewById(1)).Returns(review);

            var result = _controller.GetReviewById(1) as OkObjectResult;

            Assert.NotNull(result);
            var dto = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Null(dto.UserId);
            Assert.Null(dto.MovieTitle);
        }

        [Fact]
        public void GetReviewById_ReturnsOk_WhenSessionOrMovieIsNull()
        {
            var review = new Review
            {
                Id = 2,
                Rating = 3,
                Comment = "Sessão sem filme",
                ReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Booking = new Booking
                {
                    Id = 20,
                    UserId = 5,
                    Session = null
                }
            };

            _mockService.Setup(s => s.GetReviewById(2)).Returns(review);

            var result = _controller.GetReviewById(2) as OkObjectResult;

            Assert.NotNull(result);
            var dto = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Equal(5, dto.UserId);
            Assert.Null(dto.MovieTitle);
        }

        [Fact]
        public void CreateReview_SetsReviewDate_WhenDtoHasValue()
        {
            var dto = new ReviewCreateDto
            {
                BookingId = 5,
                Rating = 4,
                Comment = "Data fornecida",
                ReviewDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockService.Setup(s => s.CreateReview(It.IsAny<Review>()))
                .Callback<Review>(r => Assert.Equal(dto.ReviewDate!.Value.Date, r.ReviewDate.Date))
                .Returns(new Review
                {
                    Id = 1,
                    BookingId = 5,
                    Rating = 4,
                    Comment = "Data fornecida",
                    ReviewDate = dto.ReviewDate!.Value
                });

            var result = _controller.Create(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void UpdateReview_SetsReviewDate_WhenDtoHasValue()
        {
            var dto = new ReviewUpdateDto
            {
                Id = 1,
                Rating = 3,
                Comment = "Atualização com data",
                ReviewDate = DateTime.UtcNow.AddDays(-2)
            };

            _mockService.Setup(s => s.UpdateReview(1, It.IsAny<Review>()))
                .Callback<int, Review>((id, r) => Assert.Equal(dto.ReviewDate!.Value.Date, r.ReviewDate.Date))
                .Returns(new Review
                {
                    Id = 1,
                    Rating = 3,
                    Comment = "Atualização com data",
                    ReviewDate = dto.ReviewDate!.Value
                });

            var result = _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void GetReviewById_ReturnsOk_WhenMovieIsNullButSessionExists()
        {
            var review = new Review
            {
                Id = 3,
                Rating = 4,
                Comment = "Sessão sem filme",
                ReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Booking = new Booking
                {
                    Id = 10,
                    UserId = 5,
                    Session = new Session
                    {
                        Id = 99,
                        Movie = null
                    }
                }
            };

            _mockService.Setup(s => s.GetReviewById(3)).Returns(review);

            var result = _controller.GetReviewById(3) as OkObjectResult;

            Assert.NotNull(result);
            var dto = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Equal(5, dto.UserId);
            Assert.Null(dto.MovieTitle);
        }

        [Fact]
        public void GetReviewById_ReturnsOk_WhenBookingSessionAndMovieAllExist()
        {
            var review = new Review
            {
                Id = 4,
                Rating = 5,
                Comment = "Filme completo",
                ReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Booking = new Booking
                {
                    Id = 11,
                    UserId = 7,
                    Session = new Session
                    {
                        Id = 50,
                        Movie = new Movie { Id = 9, Title = "Matrix" }
                    }
                }
            };

            _mockService.Setup(s => s.GetReviewById(4)).Returns(review);

            var result = _controller.GetReviewById(4) as OkObjectResult;

            Assert.NotNull(result);
            var dto = Assert.IsType<ReviewResponseDto>(result.Value);
            Assert.Equal(7, dto.UserId);
            Assert.Equal("Matrix", dto.MovieTitle);
        }

        #endregion
    }
}
