using MovieSpot.Models;

namespace MovieSpot.Services.Invoices
{
    /// <summary>
    /// Defines contract for invoice generation services.
    /// </summary>
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(Invoice invoice);
    }
}
