using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class CinemaHall
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
