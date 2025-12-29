using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSpot.Services.Reviews;
using MovieSpot.Models;
using MovieSpot.DTO_s;
using Microsoft.AspNetCore.Authorization;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for managing reviews.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewController"/>.
        /// </summary>
        /// <param name="reviewService">Service responsible for handling review operations.</param>
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Retrieves all reviews.
        /// </summary>
        /// <returns>A collection of reviews.</returns>
        /// <response code="200">Reviews successfully retrieved.</response>
        /// <response code="404">No reviews found.</response>
        [HttpGet]
        [Authorize(Roles = "User, Staff")]
        [ProducesResponseType(typeof(IEnumerable<ReviewDTO.ReviewResponseDto>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetAllReviews()
        {
            try
            {
                var reviews = _reviewService.GetAllReviews();
                var response = reviews.Select(r => new ReviewDTO.ReviewResponseDto
                {
                    Id = r.Id,
                    BookingId = r.BookingId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserId = r.Booking?.UserId,
                    MovieTitle = r.Booking?.Session?.Movie?.Title
                });
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all reviews for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of reviews belonging to the user.</returns>
        /// <response code="200">Reviews successfully retrieved.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="404">No reviews found for the specified user.</response>
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "User, Staff")]
        [ProducesResponseType(typeof(IEnumerable<ReviewDTO.ReviewResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetAllReviewsByUserId(int userId)
        {
            try
            {
                var reviews = _reviewService.GetAllReviewsByUserId(userId);
                var response = reviews.Select(r => new ReviewDTO.ReviewResponseDto
                {
                    Id = r.Id,
                    BookingId = r.BookingId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserId = r.Booking?.UserId,
                    MovieTitle = r.Booking?.Session?.Movie?.Title
                });
                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a review by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the review.</param>
        /// <returns>The review that matches the given ID.</returns>
        /// <response code="200">Review successfully retrieved.</response>
        /// <response code="400">Invalid review ID.</response>
        /// <response code="404">Review not found.</response>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User, Staff")]
        [ProducesResponseType(typeof(ReviewDTO.ReviewResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetReviewById(int id)
        {
            try
            {
                var review = _reviewService.GetReviewById(id);
                var response = new ReviewDTO.ReviewResponseDto
                {
                    Id = review.Id,
                    BookingId = review.BookingId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    ReviewDate = review.ReviewDate,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    UserId = review.Booking?.UserId,
                    MovieTitle = review.Booking?.Session?.Movie?.Title
                };
                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new review.
        /// </summary>
        /// <param name="dto">The data required to create the review.</param>
        /// <returns>The created review.</returns>
        /// <response code="200">Review successfully created.</response>
        /// <response code="400">Invalid review data or database error.</response>
        [HttpPost]
        [Authorize(Roles = "User, Staff")]
        [ProducesResponseType(typeof(ReviewDTO.ReviewResponseDto), 200)]
        [ProducesResponseType(400)]
        public IActionResult Create([FromBody] ReviewDTO.ReviewCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Review cannot be null.");

            try
            {
                var review = new Review
                {
                    BookingId = dto.BookingId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    ReviewDate = dto.ReviewDate ?? DateTime.UtcNow
                };

                var created = _reviewService.CreateReview(review);

                var response = new ReviewDTO.ReviewResponseDto
                {
                    Id = created.Id,
                    BookingId = created.BookingId,
                    Rating = created.Rating,
                    Comment = created.Comment,
                    ReviewDate = created.ReviewDate,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing review.
        /// </summary>
        /// <param name="id">The ID of the review to update.</param>
        /// <param name="dto">The updated review data.</param>
        /// <returns>The updated review.</returns>
        /// <response code="200">Review successfully updated.</response>
        /// <response code="400">Invalid review data or ID.</response>
        /// <response code="404">Review not found.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "User, Staff")]
        [ProducesResponseType(typeof(ReviewDTO.ReviewResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Update(int id, [FromBody] ReviewDTO.ReviewUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Review cannot be null.");

            try
            {
                var review = new Review
                {
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    ReviewDate = dto.ReviewDate ?? DateTime.UtcNow
                };

                var updated = _reviewService.UpdateReview(id, review);

                var response = new ReviewDTO.ReviewResponseDto
                {
                    Id = updated.Id,
                    BookingId = updated.BookingId,
                    Rating = updated.Rating,
                    Comment = updated.Comment,
                    ReviewDate = updated.ReviewDate,
                    CreatedAt = updated.CreatedAt,
                    UpdatedAt = updated.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
