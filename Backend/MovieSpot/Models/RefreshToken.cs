using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsRevoked { get; set; } = false;

        public User User { get; set; } = null!;

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}