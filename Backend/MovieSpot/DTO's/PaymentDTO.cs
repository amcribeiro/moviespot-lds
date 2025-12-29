using System.ComponentModel.DataAnnotations;
namespace MovieSpot.DTO_s
{

    /// <summary>
    /// Request payload for initiating a payment checkout session.
    /// </summary>
    public class CreatePaymentRequestDto
    {
        /// <summary>
        /// Target booking identifier.
        /// </summary>
        [Required(ErrorMessage = "Booking ID is required.")]
        public int BookingId { get; set; }

        /// <summary>
        /// Optional voucher identifier to apply discount.
        /// </summary>
        public int? VoucherId { get; set; }
    }

    /// <summary>
    /// Response containing the Stripe hosted checkout URL.
    /// </summary>
    public class StripeIntentResponseDto
    {
        public string ClientSecret { get; set; }
    }

    /// <summary>
    /// Response representing the payment status after checking with Stripe.
    /// </summary>
    public class CheckPaymentStatusResponseDto
    {
        /// <summary>
        /// Payment status ("paid", "unpaid", "failed", "unknown").
        /// </summary>
        [Required]
        public string Status { get; set; } = "unknown";
    }

    /// <summary>
    /// DTO representing a payment record returned by the API.
    /// </summary>
    public class PaymentResponseDto
    {
        /// <summary>
        /// Unique identifier of the payment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the related booking.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Identifier of the voucher applied, if any.
        /// </summary>
        public int? VoucherId { get; set; }

        /// <summary>
        /// Payment method used (e.g., "Stripe", "MBWay", "PayPal").
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the payment ("Pending", "Paid", "Failed", etc.).
        /// </summary>
        public string PaymentStatus { get; set; } = "Pending";

        /// <summary>
        /// Date and time when the payment was made.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Amount paid by the user.
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Date when the record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the record was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
