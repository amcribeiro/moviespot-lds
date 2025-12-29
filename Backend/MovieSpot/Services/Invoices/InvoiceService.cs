using MovieSpot.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MovieSpot.Services.Invoices
{
    /// <summary>
    /// Service responsible for generating PDF invoices with booking details and QR codes.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        public InvoiceService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Generates a PDF invoice for the given booking.
        /// </summary>
        /// <param name="invoice">The invoice model containing booking and payment data.</param>
        /// <returns>A byte array representing the generated PDF.</returns>
        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            var qrContent = !string.IsNullOrWhiteSpace(invoice.ReferenceNumber)
                ? invoice.ReferenceNumber
                : invoice.BookingId ?? string.Empty;

            byte[] qrImage;
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q))
            {
                var qrCode = new PngByteQRCode(qrData);
                qrImage = qrCode.GetGraphic(pixelsPerModule: 20);
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(invoice.CompanyName).Bold().FontSize(18);
                                col.Item().Text(invoice.CompanyAddress);
                                col.Item().Text($"NIF: {invoice.CompanyNIF}");
                            });

                            row.ConstantItem(150).AlignRight().Column(col =>
                            {
                                col.Item().Text($"Fatura #{invoice.BookingId}")
                                    .Bold().FontSize(16).FontColor(Colors.Blue.Medium);
                                col.Item().Text($"Data: {invoice.PaymentDate:dd/MM/yyyy}");
                            });
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("Dados do Cliente").Bold().FontSize(14);
                            column.Item().Text($"{invoice.UserName}");
                            column.Item().Text($"{invoice.UserEmail}");
                            column.Item().PaddingTop(10);

                            column.Item().Text("Detalhes da Sessão").Bold().FontSize(14);
                            column.Item().Text($"Filme: {invoice.MovieTitle}");
                            column.Item().Text($"Sala: {invoice.CinemaHall}");
                            column.Item().Text($"Data: {invoice.SessionStart:dd/MM/yyyy HH:mm}");
                            column.Item().PaddingTop(10);

                            if (invoice.Seats.Any())
                            {
                                column.Item().Text("Lugares Reservados").Bold().FontSize(14);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.ConstantColumn(100);
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("#");
                                        header.Cell().Element(CellStyle).Text("Lugar");
                                        header.Cell().Element(CellStyle).Text("Tipo");
                                    });

                                    int i = 1;
                                    foreach (var seat in invoice.Seats)
                                    {
                                        table.Cell().Element(CellStyle).Text(i.ToString());
                                        table.Cell().Element(CellStyle).Text(seat.SeatNumber);
                                        table.Cell().Element(CellStyle).Text(seat.SeatType);
                                        i++;
                                    }

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container
                                            .PaddingVertical(4)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2);
                                    }
                                });
                            }

                            column.Item().PaddingTop(20).AlignRight().Column(col =>
                            {
                                col.Item().Text($"Subtotal: {invoice.AmountPaid:C}");
                                col.Item().Text($"IVA: {invoice.TaxAmount:C}");
                                col.Item().Text($"Total: {invoice.GrandTotal:C}").Bold();
                            });

                            column.Item().PaddingTop(30).AlignCenter().Column(qrCol =>
                            {
                                qrCol.Item().Text("Apresenta este QR Code na entrada").FontSize(12).Italic();
                                qrCol.Item().Image(qrImage).FitHeight();
                                qrCol.Item().Text($"Reserva #{invoice.BookingId}")
                                     .FontSize(10)
                                     .FontColor(Colors.Grey.Medium);
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
