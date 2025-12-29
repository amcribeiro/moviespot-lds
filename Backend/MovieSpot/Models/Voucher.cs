using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MovieSpot.Models
{

    [Index(nameof(Code), IsUnique = true)]
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0.01, 0.99)]
        [Column(TypeName = "numeric(5,2)")]
        public decimal Value { get; set; }

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime ValidUntil { get; set; }

        [Required]
        [MaxLength(16)]
        [Column(TypeName = "varchar(16)")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "smallint")]
        public int MaxUsages { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public int Usages { get; set; } = 0;

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class VoucherPerformanceDto
    {
        public string Code { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public int MaxUsages { get; set; }
        public double UsagePercentage { get; set; }
        public bool IsDepleted { get; set; }
}
    }
