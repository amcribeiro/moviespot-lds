using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Emails;
using MovieSpot.Services.Invoices;
using Stripe;
using Stripe.Checkout;

namespace MovieSpot.Services.Payments
{
    /// <summary>
    /// Provides payment management services, including Stripe integration
    /// and persistence using the application's database context.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _stripeApiKey;
        private readonly IConfiguration _configuration;
        private readonly SessionService _stripeSessionService;

        public PaymentService(
            ApplicationDbContext context,
            IConfiguration configuration,
            SessionService? stripeSessionService = null)
        {
            _context = context;
            _configuration = configuration;
            _stripeApiKey = configuration["StripeSettings:SecretKey"]
                ?? throw new InvalidOperationException("Stripe Secret Key not found in configuration.");

            StripeConfiguration.ApiKey = _stripeApiKey;
            _stripeSessionService = stripeSessionService ?? new SessionService();
        }

        /// <summary>
        /// Creates a Stripe checkout session and updates (or creates) a pending payment in the database.
        /// </summary>

        public string ProcessStripePayment(Booking booking, int? voucherId = null)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (DateTime.UtcNow > booking.CreatedAt.AddMinutes(15))
                throw new InvalidOperationException("This booking has expired and can no longer be paid.");

            if (booking.TotalAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(booking.TotalAmount));

            decimal finalAmount = booking.TotalAmount;

            if (voucherId.HasValue)
            {
                var voucher = _context.Voucher.Find(voucherId.Value)
                    ?? throw new KeyNotFoundException("Voucher not found.");

                if (voucher.ValidUntil < DateTime.UtcNow)
                    throw new InvalidOperationException("Voucher is expired.");

                // 🔥 AGORA: valor entre 0 e 1 (ex: 0.14 = 14%)
                if (voucher.Value <= 0m || voucher.Value > 1m)
                    throw new ArgumentOutOfRangeException(nameof(voucher.Value), "Voucher value must be between 0 and 1.");

                // calcula o desconto em € com arredondamento a cêntimos
                var discountValue = Math.Round(
                    finalAmount * voucher.Value,
                    2,
                    MidpointRounding.AwayFromZero
                );

                finalAmount -= discountValue;
            }

            // 🔢 garantir que o valor enviado para o Stripe está alinhado com o que guardas na BD
            var amountInCents = (long)Math.Round(
                finalAmount * 100m,
                MidpointRounding.AwayFromZero
            );

            var paymentIntentService = new PaymentIntentService();

            var intent = paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                Amount = amountInCents,           // Stripe usa CENTS
                Currency = "eur",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
        {
            { "bookingId", booking.Id.ToString() }
        }
            });

            if (intent == null || string.IsNullOrWhiteSpace(intent.ClientSecret))
                throw new Exception("Failed to create Stripe PaymentIntent.");

            var payment = new Payment
            {
                BookingId = booking.Id,
                VoucherId = voucherId,
                Reference = intent.Id,
                PaymentMethod = "Stripe",
                PaymentStatus = "Pending",
                PaymentDate = DateTime.UtcNow,
                AmountPaid = amountInCents / 100m, // 💰 exatamente o que mandaste ao Stripe
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payment.Add(payment);
            _context.SaveChanges();

            return intent.ClientSecret;
        }

        /// <summary>
        /// Checks the payment status from Stripe and updates the last pending payment accordingly.
        /// </summary>
        public async Task<string> CheckPaymentStatus(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new ArgumentException("PaymentIntent ID cannot be null or empty.", nameof(paymentIntentId));

            var intentService = new PaymentIntentService();

            var intent = await intentService.GetAsync(paymentIntentId)
                ?? throw new InvalidOperationException("Stripe PaymentIntent not found.");

            var payment = await _context.Payment
                .Include(p => p.Voucher)
                .FirstOrDefaultAsync(p => p.Reference == paymentIntentId);

            if (payment == null)
                throw new KeyNotFoundException("Payment not found for this PaymentIntent.");

            var booking = await _context.Booking
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.User)
                .Include(b => b.Session).ThenInclude(s => s.Movie)
                .Include(b => b.Session).ThenInclude(s => s.CinemaHall)
                    .ThenInclude(ch => ch.Cinema)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

            if (booking == null)
                throw new InvalidOperationException("Booking not found.");

            bool expired = DateTime.UtcNow > booking.CreatedAt.AddMinutes(15);

            // ------------------------------------------------

            if (expired)
            {
                payment.PaymentStatus = "Expired";
            }
            else if (intent.Status == "succeeded")
            {
                payment.PaymentStatus = "Paid";

                booking.Status = true;
                booking.UpdatedAt = DateTime.UtcNow;
                _context.Booking.Update(booking);

                // ✅ incrementa uso do voucher
                if (payment.VoucherId.HasValue)
                {
                    var v = payment.Voucher!;
                    v.Usages++;
                    _context.Voucher.Update(v);
                }
            }
            else if (intent.Status is "requires_payment_method" or "canceled" or "processing")
            {
                payment.PaymentStatus = "Failed";
            }

            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payment.Update(payment);
            await _context.SaveChangesAsync();

            // ------------------------------------------------
            // ✅ INVOICE + EMAIL
            // ------------------------------------------------

            if (payment.PaymentStatus == "Paid" && booking.User != null)
            {
                try
                {
                    var invoice = new MovieSpot.Models.Invoice
                    {
                        BookingId = booking.Id.ToString(),
                        UserName = booking.User.Name,
                        UserEmail = booking.User.Email,

                        PaymentMethod = payment.PaymentMethod,
                        PaymentStatus = payment.PaymentStatus,
                        AmountPaid = payment.AmountPaid,
                        TaxAmount = Math.Round(payment.AmountPaid * 0.23m, 2),
                        GrandTotal = Math.Round(payment.AmountPaid * 1.23m, 2),
                        PaymentDate = payment.PaymentDate,

                        MovieTitle = booking.Session?.Movie?.Title ?? "Filme desconhecido",
                        CinemaHall = booking.Session?.CinemaHall?.Name ?? "Sala desconhecida",
                        SessionStart = booking.Session?.StartDate ?? DateTime.UtcNow,

                        Seats = booking.BookingSeats.Select(x => x.Seat!).ToList(),

                        ReferenceNumber = payment.Reference,

                        CompanyName = booking.Session?.CinemaHall?.Cinema?.Name ?? "MovieSpot",
                        CompanyAddress =
                            $"{booking.Session?.CinemaHall?.Cinema?.Street}, {booking.Session?.CinemaHall?.Cinema?.City}",

                        CompanyNIF = "PT509999999"
                    };

                    if (!_configuration["ASPNETCORE_ENVIRONMENT"]
                        ?.Equals("Test", StringComparison.OrdinalIgnoreCase) ?? true)
                    {
                        var invoiceService = new Invoices.InvoiceService();
                        invoiceService.GenerateInvoicePdf(invoice);

                        var smtpFactory = new SmtpClientFactory();
                        var emailService = new EmailService(invoiceService, _configuration, smtpFactory);

                        await emailService.SendInvoiceAsync(invoice);
                    }
                }
                catch
                {
                    // falha de email não reverte pagamento
                }
            }

            return payment.PaymentStatus;
        }

        /// <summary>
        /// Retrieves all payments from the database.
        /// </summary>
        public List<Payment> GetAllPayments()
        {
            try
            {
                var payments = _context.Payment.ToList();

                if (payments == null || !payments.Any())
                    throw new InvalidOperationException("No payments found in the system.");

                return payments;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error while retrieving payments: {ex.Message}");
            }
        }
        public List<PaymentMethodStatDto> GetPaymentMethodStats()
        {
            return _context.Payment
                .Where(p => p.PaymentStatus == "Paid") // Só pagamentos confirmados
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodStatDto
                {
                    Method = g.Key,
                    TransactionsCount = g.Count(),
                    TotalVolume = g.Sum(p => p.AmountPaid)
                })
                .OrderByDescending(x => x.TotalVolume)
                .ToList();
        }
    }
}
