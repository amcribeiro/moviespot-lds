using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column(TypeName = "varchar(200)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 600)]
        public int Duration { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Language { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Country { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string PosterPath { get; set; } = string.Empty;

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }

    public class MovieSessionCountDto
    {
        public string MovieTitle { get; set; } = string.Empty;
        public int SessionCount { get; set; }
}

    public class MovieRevenueDto
    {
        public string MovieTitle { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
}

    public class PopularMovieDto
    {
        public string MovieTitle { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
