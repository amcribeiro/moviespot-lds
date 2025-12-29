using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string AccountStatus { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Role { get; set; } = "User";

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<Session> CreatedSessions { get; set; } = new List<Session>();
    }

    public class TopUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
}

    public class TopSpenderDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
