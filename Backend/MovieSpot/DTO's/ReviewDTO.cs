using System;
using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    /// <summary>
    /// DTOs for transferring review data between API and client.
    /// </summary>
    public class ReviewDTO
    {
        /// <summary>
        /// DTO for creating a new review.
        /// </summary>
        public class ReviewCreateDto
        {
            [Required(ErrorMessage = "Booking ID is required.")]
            public int BookingId { get; set; }

            [Required(ErrorMessage = "Rating is required.")]
            [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
            public int Rating { get; set; }

            [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
            public string? Comment { get; set; }

            public DateTime? ReviewDate { get; set; }
        }

        /// <summary>
        /// DTO for updating an existing review.
        /// </summary>
        public class ReviewUpdateDto
        {
            [Required(ErrorMessage = "Review ID is required.")]
            public int Id { get; set; }

            [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
            public int Rating { get; set; }

            [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
            public string? Comment { get; set; }

            public DateTime? ReviewDate { get; set; }
        }

        /// <summary>
        /// DTO for returning review data in responses.
        /// </summary>
        public class ReviewResponseDto
        {
            public int Id { get; set; }
            public int BookingId { get; set; }
            public int Rating { get; set; }
            public string? Comment { get; set; }
            public DateTime ReviewDate { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int? UserId { get; set; }
            public string? MovieTitle { get; set; }
        }
    }
}
