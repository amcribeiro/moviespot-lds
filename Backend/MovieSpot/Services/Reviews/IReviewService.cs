using MovieSpot.Models;

namespace MovieSpot.Services.Reviews
{
    /// <summary>
    /// Defines business operations for managing <see cref="Review"/> entities.
    /// Reviews are tied to a valid booking and limited to one review per booking.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Retrieves all reviews in the system.
        /// </summary>
        IEnumerable<Review> GetAllReviews();

        /// <summary>
        /// Retrieves all reviews written by a specific user, inferred via the review's booking.
        /// </summary>
        /// <param name="userId">User unique identifier.</param>
        IEnumerable<Review> GetAllReviewsByUserId(int userId);

        /// <summary>
        /// Retrieves a review by its identifier.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        Review GetReviewById(int id);

        /// <summary>
        /// Creates a new review. Enforces one review per booking.
        /// </summary>
        /// <param name="newReview">The review to create.</param>
        Review CreateReview(Review newReview);

        /// <summary>
        /// Updates an existing review. Does not allow changing its booking.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <param name="updatedReview">The new values for the review.</param>
        Review UpdateReview(int id, Review updatedReview);
    }
}
