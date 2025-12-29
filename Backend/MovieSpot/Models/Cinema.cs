using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Cinema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string State { get; set; } = string.Empty;

        [MaxLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Country { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();
    }
}
