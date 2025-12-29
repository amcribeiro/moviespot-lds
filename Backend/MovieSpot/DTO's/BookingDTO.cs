using System.ComponentModel.DataAnnotations;

namespace MovieSpot.DTO_s
{
    public class BookingDTO
    {
        /// <summary>
        /// DTO used for creating new bookings.
        /// Contains only the fields required for POST /Booking.
        public class BookingCreateDto
        {
            [Required]
            public int UserId { get; set; }

            [Required]
            public int SessionId { get; set; }

            [Required]
            [MinLength(1, ErrorMessage = "Tem de escolher pelo menos um lugar.")]
            public List<int> SeatIds { get; set; } = new();
        }

        /// <summary>
        /// DTO used for updating existing bookings.
        /// Contains only the fields that can be modified.
        /// </summary>
        public class BookingUpdateDto
        {
            [Required(ErrorMessage = "Booking ID is required.")]
            public int Id { get; set; }

            [Required(ErrorMessage = "User ID is required.")]
            public int UserId { get; set; }

            [Required(ErrorMessage = "Session ID is required.")]
            public int SessionId { get; set; }

            [Required(ErrorMessage = "Booking status is required.")]
            public bool Status { get; set; }

            [Required(ErrorMessage = "Total amount is required.")]
            [Range(0, double.MaxValue, ErrorMessage = "Total amount must be greater than or equal to zero.")]
            public decimal TotalAmount { get; set; }
        }

        /// <summary>
        /// DTO used for returning booking data to the client.
        /// Used in responses for GET, POST, and PUT requests.
        /// </summary>
        public class BookingResponseDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int SessionId { get; set; }
            public DateTime BookingDate { get; set; }
            public bool Status { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

    }
}
