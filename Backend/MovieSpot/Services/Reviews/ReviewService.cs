using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;

namespace MovieSpot.Services.Reviews
{
    /// <summary>
    /// Provides business logic and database operations for managing <see cref="Review"/> entities
    /// in the online cinema context. Reviews are always tied to a valid booking and
    /// constrained to one review per booking.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewService"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The application's <see cref="ApplicationDbContext"/> used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public ReviewService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves all reviews, including their related <see cref="Booking"/> information.
        /// </summary>
        /// <returns>A collection of all reviews.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no reviews exist.</exception>
        public IEnumerable<Review> GetAllReviews()
        {
            var reviews = _context.Review
                .Include(r => r.Booking)
                .AsNoTracking()
                .ToList();

            if (reviews == null || !reviews.Any())
                throw new InvalidOperationException("Não existem reviews registadas no sistema.");

            return reviews;
        }

        /// <summary>
        /// Retrieves all reviews belonging to a given user by traversing the review's booking.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>All reviews for the specified user.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If userId is not positive.</exception>
        /// <exception cref="InvalidOperationException">If no reviews are found.</exception>
        public IEnumerable<Review> GetAllReviewsByUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "O ID do utilizador deve ser maior que zero.");

            var reviews = _context.Review
                .Include(r => r.Booking)
                .Where(r => r.Booking != null && r.Booking.UserId == userId)
                .AsNoTracking()
                .ToList();

            if (reviews == null || !reviews.Any())
                throw new InvalidOperationException("Não foram encontradas reviews para este utilizador.");

            return reviews;
        }

        /// <summary>
        /// Retrieves a review by its identifier.
        /// </summary>
        /// <param name="id">The review identifier.</param>
        /// <returns>The matching review.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If id is not positive.</exception>
        /// <exception cref="KeyNotFoundException">If the review does not exist.</exception>
        public Review GetReviewById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O ID da review deve ser maior que zero.");

            var review = _context.Review
                .Include(r => r.Booking)
                .FirstOrDefault(r => r.Id == id);

            if (review == null)
                throw new KeyNotFoundException($"Review com ID {id} não encontrada.");

            return review;
        }

        /// <summary>
        /// Creates a new review while enforcing the one-review-per-booking rule.
        /// </summary>
        /// <param name="newReview">The review to create.</param>
        /// <returns>The persisted review.</returns>
        /// <exception cref="ArgumentNullException">If newReview is null.</exception>
        /// <exception cref="KeyNotFoundException">If the associated booking does not exist.</exception>
        /// <exception cref="InvalidOperationException">If the booking already has a review.</exception>
        /// <exception cref="DbUpdateException">If persistence fails.</exception>
        public Review CreateReview(Review newReview)
        {
            if (newReview == null)
                throw new ArgumentNullException(nameof(newReview), "A review não pode ser nula.");

            var booking = _context.Booking
                .Include(b => b.Review)
                .FirstOrDefault(b => b.Id == newReview.BookingId);

            if (booking == null)
                throw new KeyNotFoundException($"Reserva com ID {newReview.BookingId} não existe.");

            if (booking.Review != null)
                throw new InvalidOperationException("Cada reserva pode ter apenas uma review.");

            var now = DateTime.UtcNow;
            if (newReview.ReviewDate == default) newReview.ReviewDate = now;
            newReview.CreatedAt = now;
            newReview.UpdatedAt = now;

            try
            {
                _context.Review.Add(newReview);
                _context.SaveChanges();
                return newReview;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Erro ao guardar a nova review na base de dados.", ex);
            }
        }

        /// <summary>
        /// Updates an existing review. Changing its booking association is not allowed.
        /// </summary>
        /// <param name="id">The review identifier.</param>
        /// <param name="updatedReview">New values for the review.</param>
        /// <returns>The updated review.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If id is not positive.</exception>
        /// <exception cref="ArgumentNullException">If updatedReview is null.</exception>
        /// <exception cref="KeyNotFoundException">If the review does not exist.</exception>
        /// <exception cref="InvalidOperationException">If attempting to change the review's booking.</exception>
        /// <exception cref="DbUpdateException">If persistence fails.</exception>
        public Review UpdateReview(int id, Review updatedReview)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O ID da review deve ser maior que zero.");

            if (updatedReview == null)
                throw new ArgumentNullException(nameof(updatedReview), "A review atualizada não pode ser nula.");

            var existing = _context.Review.Find(id);
            if (existing == null)
                throw new KeyNotFoundException($"Review com ID {id} não encontrada.");

            if (updatedReview.BookingId != 0 && updatedReview.BookingId != existing.BookingId)
                throw new InvalidOperationException("Não é permitido alterar a reserva associada à review.");

            existing.Rating = updatedReview.Rating;
            existing.Comment = updatedReview.Comment;
            existing.ReviewDate = updatedReview.ReviewDate == default ? existing.ReviewDate : updatedReview.ReviewDate;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Review.Update(existing);
                _context.SaveChanges();
                return existing;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Erro ao atualizar a review na base de dados.", ex);
            }
        }
    }
}
