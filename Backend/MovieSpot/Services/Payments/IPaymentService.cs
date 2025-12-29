using MovieSpot.Models;

namespace MovieSpot.Services.Payments
{
    public interface IPaymentService
    {
        /// <summary>
        /// Retrieves all payments from the payment repository.
        /// </summary>
        /// <returns>A list of payment</returns>
        List<Payment> GetAllPayments();

        /// <summary>
        /// Creates a Stripe checkout session for a given booking, applying an optional voucher discount.
        /// </summary>
        /// <param name="booking">The booking associated with the payment.</param>
        /// <param name="voucherId">Optional voucher ID for discount application.</param>
        /// <returns>A <see cref="string"/> containing the Stripe checkout session URL.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the booking is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the total amount is invalid or voucher percentage is out of range.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the checkout session creation fails or voucher is expired.</exception>
        /// <exception cref="Stripe.StripeException">Thrown if a Stripe API error occurs.</exception>
        /// <exception cref="System.Data.Common.DbUpdateException">Thrown if saving the payment to the database fails.</exception>
        string ProcessStripePayment(Booking booking, int? voucherId = null);

        /// <summary>
        /// Checks the payment status of a Stripe checkout session and, if the payment was successful, 
        /// returns "paid", otherwise returns "unpaid".
        /// </summary>
        /// <param name="sessionId">The ID of the Stripe checkout session, required to check the payment status.</param>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronous operation. 
        /// The task result contains the payment result: "paid" if the payment was completed, or "unpaid" if it was not.
        /// </returns>
        Task<string> CheckPaymentStatus(string sessionId);

        List<PaymentMethodStatDto> GetPaymentMethodStats();
    }
}
