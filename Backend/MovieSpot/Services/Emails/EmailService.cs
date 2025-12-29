using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MovieSpot.Models;
using MovieSpot.Services.Invoices;

namespace MovieSpot.Services.Emails
{
    /// <summary>
    /// Factory interface for creating SMTP clients (used for mocking in tests).
    /// </summary>
    public interface ISmtpClientFactory
    {
        SmtpClient CreateClient();
    }

    /// <summary>
    /// Default implementation of the SMTP client factory.
    /// </summary>
    public class SmtpClientFactory : ISmtpClientFactory
    {
        public SmtpClient CreateClient() => new SmtpClient();
    }

    /// <summary>
    /// Service responsible for sending invoice emails with PDF attachments.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ISmtpClientFactory _smtpClientFactory;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// Creates an instance of <see cref="EmailService"/>.
        /// </summary>
        /// <param name="invoiceService">The service used to generate invoice PDFs.</param>
        /// <param name="configuration">The configuration containing email settings.</param>
        /// <param name="smtpClientFactory">Factory for creating SMTP clients (optional for production).</param>
        public EmailService(IInvoiceService invoiceService, IConfiguration configuration, ISmtpClientFactory? smtpClientFactory = null)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _smtpClientFactory = smtpClientFactory ?? new SmtpClientFactory();

            _smtpServer = configuration["MailSettings:SmtpServer"]!;
            _smtpPort = int.Parse(configuration["MailSettings:SmtpPort"] ?? "587");
            _fromEmail = configuration["MailSettings:FromEmail"]!;
            _fromName = configuration["MailSettings:FromName"] ?? "MovieSpot";
            _username = configuration["MailSettings:Username"]!;
            _password = configuration["MailSettings:Password"]!;
        }

        /// <summary>
        /// Sends an invoice email with the attached PDF receipt.
        /// </summary>
        /// <param name="invoice">The invoice to send.</param>
        public async Task SendInvoiceAsync(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            var pdfBytes = _invoiceService.GenerateInvoicePdf(invoice);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress(invoice.UserName, invoice.UserEmail));
            message.Subject = $"Comprovativo de Pagamento - {invoice.BookingId}";

            var body = new BodyBuilder
            {
                TextBody = $"Olá {invoice.UserName},\n\nSegue em anexo o comprovativo de pagamento.\n\nObrigado,\nEquipa MovieSpot"
            };

            body.Attachments.Add($"{invoice.BookingId}.pdf", pdfBytes, new ContentType("application", "pdf"));
            message.Body = body.ToMessageBody();

            using var client = _smtpClientFactory.CreateClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        /// <summary>
        /// Sends a generic email message using the configured SMTP client.
        /// This method can be used for password resets, notifications, etc.
        /// </summary>
        /// <param name="message">The fully constructed MimeMessage to send.</param>
        public async Task SendMimeMessageAsync(MimeMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Email message cannot be null.");

            using var client = _smtpClientFactory.CreateClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

    }
}
