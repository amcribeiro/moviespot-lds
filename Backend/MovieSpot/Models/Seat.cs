using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CinemaHallId { get; set; }
        public CinemaHall CinemaHall { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string SeatType { get; set; } = string.Empty;

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}