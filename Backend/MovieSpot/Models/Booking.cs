using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int SessionId { get; set; }
        public Session? Session { get; set; }

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool Status { get; set; } = false;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();

        public Payment? Payment { get; set; }

        public Review? Review { get; set; }

    }

    public class PeakHourDto
    {
        public int HourOfDay { get; set; } // 0 a 23
        public int BookingsMade { get; set; }
    }
}
