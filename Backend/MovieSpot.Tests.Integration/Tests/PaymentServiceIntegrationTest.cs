using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Emails;
using MovieSpot.Services.Payments;
using Stripe;
using Stripe.Checkout;

namespace MovieSpot.Tests.Integration.Services
{
    public class PaymentServiceIntegrationTest
    {
        private readonly IConfiguration _config;

        public PaymentServiceIntegrationTest()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var key = _config["StripeSettings:SecretKey"];
            Assert.False(string.IsNullOrWhiteSpace(key),
                "StripeSettings:SecretKey não encontrado. Adiciona a chave de teste da Stripe em appsettings(.Test).json");
        }

        #region Helpers

        private ApplicationDbContext NewInMemoryDb(string? name = null)
        {
            var dbName = name ?? $"PaymentInt_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static Voucher NewValidVoucher(int id = 1, decimal percent = 10m) =>
            new()
            {
                Id = id,
                Value = percent,
                ValidUntil = DateTime.UtcNow.AddDays(3)
            };

        private static Booking NewBooking(int id = 1, int total = 20, int sessionId = 123) =>
            new()
            {
                Id = id,
                TotalAmount = total,
                SessionId = sessionId
            };

        private static SessionCreateOptions NewStripeSessionOptions(decimal amount, string productName) =>
            new()
            {
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
                Mode = "payment",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmountDecimal = amount * 100,
                            Currency = "eur",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productName
                            }
                        },
                        Quantity = 1
                    }
                }
            };

        #endregion

        #region ProcessStripePayment

        /*[Fact]
        public void ProcessStripePayment_ShouldCreateSessionAndSave()
        {
            using var ctx = NewInMemoryDb();
            var service = new PaymentService(ctx, _config);
            var booking = NewBooking(id: 10, total: 20);

            var url = service.ProcessStripePayment(booking);

            Assert.NotNull(url);
            Assert.StartsWith("https://checkout.stripe.com", url);

            var payment = ctx.Payment.FirstOrDefault();
            Assert.NotNull(payment);
            Assert.Equal("Pending", payment!.PaymentStatus);
            Assert.Equal(20m, payment.AmountPaid);
        }*/

        /*[Fact]
        public void ProcessStripePayment_WithValidVoucher_ShouldApplyDiscount_AndSave()
        {
            using var ctx = NewInMemoryDb();
            ctx.Voucher.Add(NewValidVoucher(id: 1, percent: 25m));
            ctx.SaveChanges();

            var service = new PaymentService(ctx, _config);
            var booking = NewBooking(id: 11, total: 40);

            var url = service.ProcessStripePayment(booking, 1);

            Assert.NotNull(url);
            Assert.StartsWith("https://checkout.stripe.com", url);

            var payment = ctx.Payment.First();
            Assert.Equal(30m, payment.AmountPaid);
            Assert.Equal("Pending", payment.PaymentStatus);
        }*/

        [Fact]
        public void ProcessStripePayment_WithNullVoucher_ShouldThrowKeyNotFoundException()
        {
            using var ctx = NewInMemoryDb();
            var service = new PaymentService(ctx, _config);
            var booking = NewBooking();

            Assert.Throws<KeyNotFoundException>(() =>
                service.ProcessStripePayment(booking, 9999));
        }

        [Fact]
        public void ProcessStripePayment_WithExpiredVoucher_ShouldThrow()
        {
            using var ctx = NewInMemoryDb();
            ctx.Voucher.Add(new Voucher
            {
                Id = 2,
                Value = 10m,
                ValidUntil = DateTime.UtcNow.AddDays(-1)
            });
            ctx.SaveChanges();

            var service = new PaymentService(ctx, _config);
            var booking = NewBooking();

            Assert.Throws<InvalidOperationException>(() =>
                service.ProcessStripePayment(booking, 2));
        }

        [Fact]
        public void ProcessStripePayment_WithZeroAmount_ShouldThrow()
        {
            using var ctx = NewInMemoryDb();
            var service = new PaymentService(ctx, _config);
            var booking = NewBooking(total: 0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                service.ProcessStripePayment(booking));
        }

        [Fact]
        public void ProcessStripePayment_NullBooking_ShouldThrow()
        {
            using var ctx = NewInMemoryDb();
            var service = new PaymentService(ctx, _config);

            Assert.Throws<ArgumentNullException>(() =>
                service.ProcessStripePayment(null!));
        }

        #endregion

        #region CheckPaymentStatus

        [Fact]
        public async Task CheckPaymentStatus_WithInvalidSessionId_ShouldThrowStripeException()
        {
            using var ctx = NewInMemoryDb();
            ctx.Payment.Add(new Payment
            {
                BookingId = 999,
                PaymentMethod = "Stripe",
                PaymentStatus = "Pending",
                AmountPaid = 10m,
                PaymentDate = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var service = new PaymentService(ctx, _config);

            await Assert.ThrowsAsync<StripeException>(async () =>
                await service.CheckPaymentStatus("cs_test_invalid_id"));
        }

        /*[Fact]
        public async Task CheckPaymentStatus_WithRealStripeSession_ShouldWorkCorrectly()
        {
            using var ctx = NewInMemoryDb();

            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
            var sessionService = new Stripe.Checkout.SessionService();
            var stripeSession = sessionService.Create(NewStripeSessionOptions(5, "Integration Test Session"));

            var booking = NewBooking(id: 21, total: 5);
            ctx.Booking.Add(booking);
            ctx.Payment.Add(new Payment
            {
                BookingId = booking.Id,
                PaymentMethod = "Stripe",
                PaymentStatus = "Pending",
                AmountPaid = 5m,
                PaymentDate = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var service = new PaymentService(ctx, _config);
            var result = await service.CheckPaymentStatus(stripeSession.Id);

            var updated = ctx.Payment.First();
            Assert.Contains(result, new[] { "open", "unpaid", "paid", "no_payment_required" });
            Assert.Contains(updated.PaymentStatus, new[] { "Pending", "Failed", "Paid" });
        }*/

        /*[Fact]
        public async Task CheckPaymentStatus_WhenBookingAndUserExist_ShouldGenerateInvoiceAndSendEmail()
        {
            using var ctx = NewInMemoryDb();

            var user = new User { Id = 1, Name = "Test User", Email = "user@test.com" };
            var booking = NewBooking(id: 1, total: 20);
            ctx.User.Add(user);
            ctx.Booking.Add(booking);
            ctx.Payment.Add(new Payment
            {
                BookingId = booking.Id,
                PaymentMethod = "Stripe",
                PaymentStatus = "Pending",
                AmountPaid = 20m,
                PaymentDate = DateTime.UtcNow
            });
            ctx.SaveChanges();

            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
            var sessionService = new Stripe.Checkout.SessionService();
            var stripeSession = sessionService.Create(NewStripeSessionOptions(20, "Invoice Integration Test"));

            var service = new PaymentService(ctx, _config);
            var result = await service.CheckPaymentStatus(stripeSession.Id);

            Assert.Contains(result, new[] { "open", "unpaid", "paid", "no_payment_required" });
            var updated = ctx.Payment.First();
            Assert.Contains(updated.PaymentStatus, new[] { "Pending", "Failed", "Paid" });
        }*/

        [Fact]
        public async Task CheckPaymentStatus_WhenStripeKeyMissing_ShouldThrow()
        {
            using var ctx = NewInMemoryDb();
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await new PaymentService(ctx, badConfig).CheckPaymentStatus("cs_test_invalid"));

            Assert.Equal("Stripe Secret Key not found in configuration.", ex.Message);
        }

        //// [Fact]
        //// public async Task CheckPaymentStatus_WhenBookingHasUserAndSeats_ShouldGenerateInvoiceAndSendEmail()
        //// {
        //    
        //    using var ctx = NewInMemoryDb();

        //    
        //    var user = new User
        //    {
        //        Id = 1,
        //        Name = "Ana Silva",
        //        Email = "ana@teste.com",
        //        Password = "hashed_password",
        //        Phone = "912345678",
        //        Role = "user",
        //        AccountStatus = "active"
        //    };

        //    var cinema = new Cinema
        //    {
        //        Id = 1,
        //        Name = "MovieSpot Cinema",
        //        Street = "Rua XPTO 12",
        //        City = "Lisboa"
        //    };

        //    var hall = new CinemaHall
        //    {
        //        Id = 1,
        //        Name = "Sala 3",
        //        Cinema = cinema
        //    };

        //    var movie = new Movie
        //    {
        //        Id = 1,
        //        Title = "Matrix Reloaded",
        //        Description = "Neo fights again",
        //        Duration = 120,
        //        ReleaseDate = new DateTime(2003, 5, 15),
        //        Language = "EN",
        //        Country = "USA"
        //    };

        //    var session = new Models.Session
        //    {
        //        Id = 1,
        //        Movie = movie,
        //        CinemaHall = hall,
        //        CreatedBy = user.Id,
        //        StartDate = DateTime.UtcNow.AddHours(2),
        //        EndDate = DateTime.UtcNow.AddHours(4),
        //        Price = 12.50m
        //    };

        //    var seat1 = new Seat
        //    {
        //        Id = 1,
        //        CinemaHall = hall,
        //        CinemaHallId = hall.Id,
        //        SeatNumber = "A1",
        //        SeatType = "Normal"
        //    };

        //    var seat2 = new Seat
        //    {
        //        Id = 2,
        //        CinemaHall = hall,
        //        CinemaHallId = hall.Id,
        //        SeatNumber = "A2",
        //        SeatType = "Normal"
        //    };

        //    var booking = new Booking
        //    {
        //        Id = 10,
        //        User = user,
        //        UserId = user.Id,
        //        Session = session,
        //        SessionId = session.Id,
        //        TotalAmount = 25,
        //        BookingSeats = new List<BookingSeat>
        //{
        //    new() { Seat = seat1, SeatId = seat1.Id, SeatPrice = 12.50m },
        //    new() { Seat = seat2, SeatId = seat2.Id, SeatPrice = 12.50m }
        //}
        //    };

        //    ctx.Booking.Add(booking);
        //    ctx.Payment.Add(new Payment
        //    {
        //        BookingId = booking.Id,
        //        PaymentMethod = "Stripe",
        //        PaymentStatus = "Pending",
        //        AmountPaid = 25m,
        //        PaymentDate = DateTime.UtcNow,
        //        CreatedAt = DateTime.UtcNow
        //    });
        //    ctx.SaveChanges();

        //    
        //    StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
        //    var stripeService = new Stripe.Checkout.SessionService();
        //    var stripeSession = stripeService.Create(new Stripe.Checkout.SessionCreateOptions
        //    {
        //        SuccessUrl = "https://example.com/success",
        //        CancelUrl = "https://example.com/cancel",
        //        Mode = "payment",
        //        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        //{
        //    new()
        //    {
        //        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
        //        {
        //            UnitAmountDecimal = 2500,
        //            Currency = "eur",
        //            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
        //            {
        //                Name = "Matrix Reloaded Ticket"
        //            }
        //        },
        //        Quantity = 1
        //    }
        //}
        //    });
        //    var invoiceService = new MovieSpot.Services.Invoices.InvoiceService();
        //    var emailService = new MovieSpot.Services.Emails.EmailService(
        //        invoiceService, _config, new MovieSpot.Services.Emails.SmtpClientFactory()
        //    );

        //    var service = new PaymentService(ctx, _config);

        //    var result = await service.CheckPaymentStatus(stripeSession.Id);

        //    Assert.Contains(result, new[] { "open", "unpaid", "paid", "no_payment_required" });

        //    var updated = ctx.Payment.First();
        //    Assert.Contains(updated.PaymentStatus, new[] { "Pending", "Failed", "Paid" });

        //    Console.WriteLine($"Pagamento atualizado com estado: {updated.PaymentStatus}");
        // }


        #endregion
    }
}
