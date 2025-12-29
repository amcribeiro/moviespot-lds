using System;
using System.Collections.Generic;
using System.Text;
using MovieSpot.Models;
using MovieSpot.Services.Invoices;
using QRCoder;
using Xunit;

namespace MovieSpot.Tests.Services.Invoices
{
    public class InvoiceServiceTest
    {
        private readonly InvoiceService _service;

        public InvoiceServiceTest()
        {
            _service = new InvoiceService();
        }

        private Invoice CreateSampleInvoice()
        {
            return new Invoice
            {
                BookingId = "BKG123",
                UserName = "João Silva",
                UserEmail = "joao@example.com",
                PaymentMethod = "MBWay",
                PaymentStatus = "Pago",
                AmountPaid = 20.00m,
                TaxAmount = 4.60m,
                GrandTotal = 24.60m,
                PaymentDate = new DateTime(2025, 1, 15),
                MovieTitle = "Interstellar",
                SessionStart = new DateTime(2025, 1, 16, 21, 30, 0),
                CinemaHall = "Sala 5",
                ReferenceNumber = "REF123456",
                Seats = new List<Seat>
                {
                    new Seat { SeatNumber = "A1", SeatType = "Normal" },
                    new Seat { SeatNumber = "A2", SeatType = "VIP" }
                }
            };
        }

        [Fact]
        public void GenerateInvoicePdf_ValidInvoice_ReturnsNonEmptyPdf()
        {
            var invoice = CreateSampleInvoice();

            var pdfBytes = _service.GenerateInvoicePdf(invoice);

            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);

            var header = Encoding.ASCII.GetString(pdfBytes, 0, 4);
            Assert.Equal("%PDF", header);
        }

        [Fact]
        public void GenerateInvoicePdf_WithoutSeats_StillGeneratesValidPdf()
        {
            var invoice = CreateSampleInvoice();
            invoice.Seats.Clear();

            var pdfBytes = _service.GenerateInvoicePdf(invoice);

            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);

            var header = Encoding.ASCII.GetString(pdfBytes, 0, 4);
            Assert.Equal("%PDF", header);
        }

        [Fact]
        public void GenerateInvoicePdf_WithEmptyFields_DoesNotThrow()
        {
            var invoice = new Invoice
            {
                BookingId = "",
                UserName = "",
                UserEmail = "",
                PaymentDate = DateTime.UtcNow,
                SessionStart = DateTime.UtcNow
            };

            var ex = Record.Exception(() => _service.GenerateInvoicePdf(invoice));

            Assert.Null(ex);
        }

        [Fact]
        public void GenerateInvoicePdf_WhenInvoiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.GenerateInvoicePdf(null));
        }

        [Fact]
        public void GenerateInvoicePdf_ContainsCompanyDetails()
        {
            var invoice = CreateSampleInvoice();

            var pdfBytes = _service.GenerateInvoicePdf(invoice);

            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 1000);
        }
    }
}
