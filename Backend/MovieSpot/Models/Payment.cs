using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieSpot.Models
{
    public class Payment
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int? VoucherId { get; set; }
        public Voucher? Voucher { get; set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Reference { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        [Column(TypeName = "timestamptz")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PaymentMethodStatDto
    {
        public string Method { get; set; } = string.Empty;
        public int TransactionsCount { get; set; }
        public decimal TotalVolume { get; set; }
    }
}
