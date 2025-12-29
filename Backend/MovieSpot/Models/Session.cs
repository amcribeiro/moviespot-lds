using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }

        [Required]
        public int CinemaHallId { get; set; }
        public CinemaHall? CinemaHall { get; set; }

        [Required]
        public int CreatedBy { get; set; }
        public User? CreatedByUser { get; set; }

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime EndDate { get; set; }


        [Required]
        [Column(TypeName = "decimal(6,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }


        [Column(TypeName = "smallint")]
        [Range(0, 100)]
        public int? PromotionValue { get; set; }

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
    public class SessionOccupancyDto
    {
        public int SessionId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int BookedSeats { get; set; }
        public double OccupancyRate { get; set; }
    }
}
