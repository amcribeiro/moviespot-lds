using MimeKit;
using MovieSpot.Models;

namespace MovieSpot.Services.Emails

{
    /// <summary>
    /// Defines the contract for sending emails within the MovieSpot application.
    /// This interface allows for flexible implementations (e.g., real SMTP service or mock service for testing).
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an invoice email to the user with the corresponding PDF receipt attached.
        /// Typically used after a successful payment.
        /// </summary>
        /// <param name="invoice">
        /// The <see cref="Invoice"/> object containing the recipient information and payment details.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous email sending operation.
        /// </returns>
        Task SendInvoiceAsync(Invoice invoice);

        /// <summary>
        /// Sends a general-purpose email using a pre-constructed <see cref="MimeMessage"/>.
        /// This method can be used for password resets, notifications, or any other system emails.
        /// </summary>
        /// <param name="message">
        /// The <see cref="MimeMessage"/> object that defines the sender, recipient, subject, and body of the email.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous email sending operation.
        /// </returns>
        Task SendMimeMessageAsync(MimeMessage message);
    }
}
