using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    public class SeatDTO
    {
        /// <summary>
        /// DTO used for creating new seats.
        /// Contains only the fields required for POST /Seat.
        /// </summary>
        public class SeatCreateDto
        {
            [Required(ErrorMessage = "CinemaHallId is required.")]
            public int CinemaHallId { get; set; }

            [Required(ErrorMessage = "Seat number is required.")]
            [MaxLength(10, ErrorMessage = "Seat number cannot exceed 10 characters.")]
            public string SeatNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Seat type is required.")]
            [MaxLength(50, ErrorMessage = "Seat type cannot exceed 50 characters.")]
            public string SeatType { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO used for updating existing seats.
        /// Contains only the fields that can be modified.
        /// </summary>
        public class SeatUpdateDto
        {
            [Required(ErrorMessage = "Seat ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "CinemaHallId is required.")]
            public int CinemaHallId { get; set; }

            [Required(ErrorMessage = "Seat number is required.")]
            [MaxLength(10, ErrorMessage = "Seat number cannot exceed 10 characters.")]
            public string SeatNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Seat type is required.")]
            [MaxLength(50, ErrorMessage = "Seat type cannot exceed 50 characters.")]
            public string SeatType { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO used for returning seat data to the client.
        /// Used in responses for GET, POST, and PUT requests.
        /// </summary>
        public class SeatResponseDto
        {
            public int Id { get; set; }
            public int CinemaHallId { get; set; }
            public string SeatNumber { get; set; } = string.Empty;
            public string SeatType { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class SeatResponsePriceDto
        {
            public int Id { get; set; }
            public int CinemaHallId { get; set; }
            public string SeatNumber { get; set; } = string.Empty;
            public string SeatType { get; set; } = string.Empty;
            public double Price { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
