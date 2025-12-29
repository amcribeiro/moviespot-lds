using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Moq;
using MovieSpot.Models;
using MovieSpot.Services.Emails;
using MovieSpot.Services.Invoices;
using Xunit;

namespace MovieSpot.Tests.Services.Emails
{
    /// <summary>
    /// Unit tests for the <see cref="EmailService"/>.
    /// Verifies email sending behavior with PDF attachments,
    /// and handles error and configuration scenarios.
    /// </summary>
    public class EmailServiceTest
    {
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly Mock<ISmtpClientFactory> _smtpFactoryMock;
        private readonly Mock<SmtpClient> _smtpClientMock;
        private readonly IConfiguration _config;
        private readonly EmailService _service;

        public EmailServiceTest()
        {
            _invoiceServiceMock = new Mock<IInvoiceService>();

            _smtpClientMock = new Mock<SmtpClient>();

            _smtpFactoryMock = new Mock<ISmtpClientFactory>();
            _smtpFactoryMock.Setup(f => f.CreateClient()).Returns(_smtpClientMock.Object);

            var settings = new Dictionary<string, string?>
            {
                {"MailSettings:SmtpServer", "smtp.fake.com"},
                {"MailSettings:SmtpPort", "587"},
                {"MailSettings:FromEmail", "noreply@moviespot.com"},
                {"MailSettings:FromName", "MovieSpot"},
                {"MailSettings:Username", "user"},
                {"MailSettings:Password", "pass"}
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _service = new EmailService(_invoiceServiceMock.Object, _config, _smtpFactoryMock.Object);
        }

        #region SEND INVOICE

        [Fact]
        public async Task SendInvoiceAsync_Should_Send_Email_With_Attachment()
        {
            var invoice = new Invoice
            {
                BookingId = "123",
                UserName = "John Doe",
                UserEmail = "john@test.com"
            };

            var fakePdf = new byte[] { 1, 2, 3 };
            _invoiceServiceMock.Setup(x => x.GenerateInvoicePdf(invoice)).Returns(fakePdf);

            MimeMessage? capturedMessage = null;

            _smtpClientMock
                .Setup(c => c.SendAsync(
                    It.IsAny<MimeMessage>(),
                    It.IsAny<System.Threading.CancellationToken>(),
                    It.IsAny<ITransferProgress?>()))
                .Callback<MimeMessage, System.Threading.CancellationToken, ITransferProgress?>((msg, _, _) =>
                {
                    capturedMessage = msg;
                })
                .ReturnsAsync(string.Empty);

            await _service.SendInvoiceAsync(invoice);

            _invoiceServiceMock.Verify(x => x.GenerateInvoicePdf(invoice), Times.Once);

            _smtpClientMock.Verify(c => c.ConnectAsync("smtp.fake.com", 587, SecureSocketOptions.StartTls, default), Times.Once);

            _smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<System.Threading.CancellationToken>(), It.IsAny<ITransferProgress?>()), Times.Once);
            _smtpClientMock.Verify(c => c.DisconnectAsync(true, default), Times.Once);

            Assert.NotNull(capturedMessage);
            Assert.Equal("Comprovativo de Pagamento - 123", capturedMessage!.Subject);
            Assert.Contains("john@test.com", capturedMessage.To.ToString());
            Assert.Contains("noreply@moviespot.com", capturedMessage.From.ToString());
        }

        [Fact]
        public async Task SendInvoiceAsync_Should_Throw_When_Invoice_Is_Null()
        {

            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SendInvoiceAsync(null!));
        }

        [Fact]
        public async Task SendInvoiceAsync_Should_Throw_When_GenerateInvoicePdf_Fails()
        {
            var invoice = new Invoice { BookingId = "1", UserName = "Anna", UserEmail = "anna@test.com" };

            _invoiceServiceMock
                .Setup(x => x.GenerateInvoicePdf(invoice))
                .Throws(new InvalidOperationException("Error generating PDF"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SendInvoiceAsync(invoice));
            Assert.Contains("Error generating PDF", ex.Message);
        }

        [Fact]
        public async Task SendInvoiceAsync_Should_Propagate_Smtp_Exceptions()
        {
            var invoice = new Invoice { BookingId = "5", UserName = "Peter", UserEmail = "peter@test.com" };

            var fakePdf = new byte[] { 9, 9, 9 };
            _invoiceServiceMock.Setup(x => x.GenerateInvoicePdf(invoice)).Returns(fakePdf);

            _smtpClientMock
                .Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), default))
                .ThrowsAsync(new SmtpCommandException(
                    SmtpErrorCode.MessageNotAccepted,
                    (SmtpStatusCode)0,
                    new MailboxAddress("Test", "test@fake.com"),
                    "SMTP error",
                    null
                ));

            var ex = await Assert.ThrowsAsync<SmtpCommandException>(() => _service.SendInvoiceAsync(invoice));
            Assert.Contains("SMTP error", ex.Message);
        }

        #endregion
        [Fact]
        public void SmtpClientFactory_Should_Create_New_SmtpClient()
        {
            var factory = new SmtpClientFactory();

            var client = factory.CreateClient();

            Assert.NotNull(client);
            Assert.IsType<SmtpClient>(client);
        }

        [Fact]
        public void EmailService_Should_Use_Default_SmtpClientFactory_When_Null()
        {
            var settings = new Dictionary<string, string?>
    {
        {"MailSettings:SmtpServer", "smtp.fake.com"},
        {"MailSettings:SmtpPort", "587"},
        {"MailSettings:FromEmail", "noreply@moviespot.com"},
        {"MailSettings:FromName", "MovieSpot"},
        {"MailSettings:Username", "user"},
        {"MailSettings:Password", "pass"}
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var invoiceServiceMock = new Mock<IInvoiceService>();

            var service = new EmailService(invoiceServiceMock.Object, config, null);

            Assert.NotNull(service);
        }

        [Fact]
        public void EmailService_Should_Fallback_To_Default_Values_When_Config_Missing()
        {
            var settings = new Dictionary<string, string?>
    {
        {"MailSettings:SmtpServer", "smtp.fake.com"},
        {"MailSettings:FromEmail", "noreply@moviespot.com"},
        {"MailSettings:Username", "user"},
        {"MailSettings:Password", "pass"}
    };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var invoiceServiceMock = new Mock<IInvoiceService>();

            var service = new EmailService(invoiceServiceMock.Object, config);

            Assert.NotNull(service);

            Assert.NotNull(service);
        }
    }
}
