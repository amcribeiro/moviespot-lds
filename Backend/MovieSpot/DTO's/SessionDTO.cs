using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    public class SessionDTO
    {
        /// <summary>
        /// DTO used for creating new sessions.
        /// </summary>
        public class SessionCreateDto
        {
            [Required(ErrorMessage = "Movie ID is required.")]
            public int MovieId { get; set; }

            [Required(ErrorMessage = "Cinema hall ID is required.")]
            public int CinemaHallId { get; set; }

            [Required(ErrorMessage = "Creator user ID is required.")]
            public int CreatedBy { get; set; }

            [Required(ErrorMessage = "Start date is required.")]
            public DateTime StartDate { get; set; }

            [Required(ErrorMessage = "End date is required.")]
            public DateTime EndDate { get; set; }

            [Required(ErrorMessage = "Price is required.")]
            [Range(0, double.MaxValue, ErrorMessage = "Price must be positive.")]
            public decimal Price { get; set; }
        }

        /// <summary>
        /// DTO used for updating existing sessions.
        /// </summary>
        public class SessionUpdateDto
        {
            [Required(ErrorMessage = "Session ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "Movie ID is required.")]
            public int MovieId { get; set; }

            [Required(ErrorMessage = "Cinema hall ID is required.")]
            public int CinemaHallId { get; set; }

            [Required(ErrorMessage = "Start date is required.")]
            public DateTime StartDate { get; set; }

            [Required(ErrorMessage = "End date is required.")]
            public DateTime EndDate { get; set; }

            [Required(ErrorMessage = "Price is required.")]
            [Range(0, double.MaxValue, ErrorMessage = "Price must be positive.")]
            public decimal Price { get; set; }
        }

        /// <summary>
        /// DTO used for returning session data to the client.
        /// </summary>
        public class SessionResponseDto
        {
            public int Id { get; set; }
            public int MovieId { get; set; }
            public string MovieTitle { get; set; } = string.Empty;
            public int CinemaHallId { get; set; }
            public string CinemaHallName { get; set; } = string.Empty;
            public int CreatedBy { get; set; }
            public string CreatedByName { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal Price { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
        /// <summary>
        /// DTO used for returning available seats for a session.
        /// </summary>
        public class AvailableSeatDto
        {
            public int Id { get; set; }
            public string SeatNumber { get; set; } = string.Empty;
            public string SeatType { get; set; } = string.Empty;
        }
    }
}
