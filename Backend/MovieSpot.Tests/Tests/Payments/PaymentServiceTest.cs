using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Payments;
using Stripe.Checkout;
using Xunit;

namespace MovieSpot.Tests.Services.Payments
{
    public class PaymentServiceTest
    {
        private ApplicationDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private IConfiguration GetFakeConfig() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "StripeSettings:SecretKey", "sk_test_fake" }
                })
                .Build();

        #region GetAllPayments Tests

        [Fact]
        public void GetAllPayments_WhenEmpty_ShouldThrowInvalidOperationException()
        {
            var ctx = GetInMemoryDb();
            var service = new PaymentService(ctx, GetFakeConfig());

            var ex = Assert.Throws<InvalidOperationException>(() => service.GetAllPayments());
            Assert.Equal("No payments found in the system.", ex.Message);
        }

        [Fact]
        public void GetAllPayments_WhenHasData_ShouldReturnListOfPayments()
        {
            var ctx = GetInMemoryDb();
            ctx.Payment.AddRange(
                new Payment { Id = 1, AmountPaid = 10, PaymentStatus = "Paid" },
                new Payment { Id = 2, AmountPaid = 15, PaymentStatus = "Pending" }
            );
            ctx.SaveChanges();

            var service = new PaymentService(ctx, GetFakeConfig());

            var payments = service.GetAllPayments();

            Assert.NotNull(payments);
            Assert.Equal(2, payments.Count);
            Assert.Contains(payments, p => p.PaymentStatus == "Paid");
            Assert.Contains(payments, p => p.PaymentStatus == "Pending");
        }

        #endregion

        #region ProcessStripePayment Tests

        [Fact]
        public void ProcessStripePayment_WhenBookingIsNull_ShouldThrowArgumentNullException()
        {
            var ctx = GetInMemoryDb();
            var service = new PaymentService(ctx, GetFakeConfig());

            Assert.Throws<ArgumentNullException>(() => service.ProcessStripePayment(null!));
        }

        [Fact]
        public void ProcessStripePayment_WhenBookingAmountIsZero_ShouldThrowArgumentOutOfRangeException()
        {
            var ctx = GetInMemoryDb();
            var service = new PaymentService(ctx, GetFakeConfig());

            var booking = new Booking { Id = 1, TotalAmount = 0 };

            Assert.Throws<ArgumentOutOfRangeException>(() => service.ProcessStripePayment(booking));
        }

        [Fact]
        public void ProcessStripePayment_WhenVoucherInvalid_ShouldThrowKeyNotFoundException()
        {
            var ctx = GetInMemoryDb();
            var service = new PaymentService(ctx, GetFakeConfig());

            var booking = new Booking { Id = 1, SessionId = 1, TotalAmount = 10 };

            Assert.Throws<KeyNotFoundException>(() => service.ProcessStripePayment(booking, 99));
        }

        [Fact]
        public void ProcessStripePayment_WhenVoucherExpired_ShouldThrowInvalidOperationException()
        {
            var ctx = GetInMemoryDb();
            ctx.Voucher.Add(new Voucher
            {
                Id = 1,
                Code = "1111",
                Value = 10,
                ValidUntil = DateTime.UtcNow.AddDays(-1)
            });
            ctx.SaveChanges();

            var service = new PaymentService(ctx, GetFakeConfig());
            var booking = new Booking { Id = 1, SessionId = 5, TotalAmount = 10 };

            Assert.Throws<InvalidOperationException>(() => service.ProcessStripePayment(booking, 1));
        }

        /*[Fact]
        public void ProcessStripePayment_WithValidData_ShouldCreatePaymentAndReturnUrl()
        {
            var ctx = GetInMemoryDb();
            var config = GetFakeConfig();

            var fakeSession = new Stripe.Checkout.Session { Url = "https://fake.stripe.checkout" };
            var mockStripe = new Mock<SessionService>();
            mockStripe.Setup(s => s.Create(It.IsAny<SessionCreateOptions>(), null))
                      .Returns(fakeSession);

            var booking = new Booking { Id = 1, SessionId = 5, TotalAmount = 25 };
            var service = new PaymentService(ctx, config, mockStripe.Object);

            var result = service.ProcessStripePayment(booking);

            Assert.Equal("https://fake.stripe.checkout", result);
            var payment = ctx.Payment.FirstOrDefault();
            Assert.NotNull(payment);
            Assert.Equal(25, payment.AmountPaid);
            Assert.Equal("Pending", payment.PaymentStatus);
        }*/

        /*[Fact]
        public void ProcessStripePayment_WithVoucher_ShouldApplyDiscountCorrectly()
        {
            var ctx = GetInMemoryDb();
            ctx.Voucher.Add(new Voucher
            {
                Id = 1,
                Code = "123",
                Value = 20,
                ValidUntil = DateTime.UtcNow.AddDays(1)
            });
            ctx.SaveChanges();

            var fakeSession = new Stripe.Checkout.Session { Url = "https://fake.stripe.checkout" };
            var mockStripe = new Mock<SessionService>();
            mockStripe.Setup(s => s.Create(It.IsAny<SessionCreateOptions>(), null))
                      .Returns(fakeSession);

            var booking = new Booking { Id = 1, SessionId = 7, TotalAmount = 100 };
            var service = new PaymentService(ctx, GetFakeConfig(), mockStripe.Object);

            var url = service.ProcessStripePayment(booking, 1);

            Assert.Equal("https://fake.stripe.checkout", url);
            var payment = ctx.Payment.First();
            Assert.Equal(80, payment.AmountPaid); // 20% discount
            Assert.Equal("Pending", payment.PaymentStatus);
        }*/

        #endregion

        #region CheckPaymentStatus Tests

        /*[Fact]
        public async Task CheckPaymentStatus_WhenSessionIsPaid_ShouldUpdateToPaid()
        {
            var ctx = GetInMemoryDb();
            ctx.Payment.Add(new Payment
            {
                Id = 1,
                BookingId = 1,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var fakeSession = new Stripe.Checkout.Session
            {
                Id = "sess_123",
                PaymentStatus = "paid"
            };

            var mockStripe = new Mock<SessionService>();
            mockStripe.Setup(s => s.Get("sess_123", null, null))
                      .Returns(fakeSession);

            var service = new PaymentService(ctx, GetFakeConfig(), mockStripe.Object);

            var status = await service.CheckPaymentStatus("sess_123");

            Assert.Equal("paid", status);
            var payment = ctx.Payment.First();
            Assert.Equal("Paid", payment.PaymentStatus);
        }*/

        /*[Fact]
        public async Task CheckPaymentStatus_WhenSessionIsFailed_ShouldUpdateToFailed()
        {
            var ctx = GetInMemoryDb();
            ctx.Payment.Add(new Payment
            {
                Id = 1,
                BookingId = 1,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var fakeSession = new Stripe.Checkout.Session
            {
                Id = "sess_456",
                PaymentStatus = "failed"
            };

            var mockStripe = new Mock<SessionService>();
            mockStripe.Setup(s => s.Get("sess_456", null, null))
                      .Returns(fakeSession);

            var service = new PaymentService(ctx, GetFakeConfig(), mockStripe.Object);

            var status = await service.CheckPaymentStatus("sess_456");

            Assert.Equal("failed", status);
            var payment = ctx.Payment.First();
            Assert.Equal("Failed", payment.PaymentStatus);
        }*/

        [Fact]
        public async Task CheckPaymentStatus_WhenSessionIdIsEmpty_ShouldThrowArgumentException()
        {
            var ctx = GetInMemoryDb();
            var service = new PaymentService(ctx, GetFakeConfig());

            await Assert.ThrowsAsync<ArgumentException>(() => service.CheckPaymentStatus(""));
        }

        /*[Fact]
        public async Task CheckPaymentStatus_WhenNoPendingPayment_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var ctx = GetInMemoryDb();
            // Nenhum pagamento pendente no DB
            ctx.Payment.Add(new Payment
            {
                Id = 1,
                PaymentStatus = "Paid", // não é Pending
                CreatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var fakeSession = new Stripe.Checkout.Session
            {
                Id = "sess_999",
                PaymentStatus = "paid"
            };

            var mockStripe = new Mock<Stripe.Checkout.SessionService>();
            mockStripe.Setup(s => s.Get("sess_999", null, null))
                      .Returns(fakeSession);

            var service = new PaymentService(ctx, GetFakeConfig(), mockStripe.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CheckPaymentStatus("sess_999"));
            Assert.Equal("No matching pending payment found in the database.", ex.Message);
        }*/

        /*[Fact]
        public async Task CheckPaymentStatus_WhenBookingAndUserExist_ShouldCompleteWithoutError()
        {
            var ctx = GetInMemoryDb();

            var user = new User
            {
                Id = 1,
                Name = "Ana Silva",
                Email = "ana@teste.com",
                Password = "hashed_password",
                Phone = "912345678",
                Role = "user",
                AccountStatus = "active"
            };

            var cinema = new Cinema
            {
                Id = 1,
                Name = "MovieSpot Cinema",
                Street = "Rua XPTO 12",
                City = "Lisboa"
            };

            var hall = new CinemaHall
            {
                Id = 1,
                Name = "Sala 3",
                Cinema = cinema
            };

            var movie = new Movie
            {
                Id = 1,
                Title = "Matrix Reloaded",
                Description = "Neo fights again",
                Duration = 120,
                ReleaseDate = new DateTime(2003, 5, 15),
                Language = "EN",
                Country = "USA"
            };

            var session = new Models.Session
            {
                Id = 1,
                Movie = movie,
                CinemaHall = hall,
                CreatedBy = user.Id,
                StartDate = DateTime.UtcNow.AddHours(2),
                EndDate = DateTime.UtcNow.AddHours(4),
                Price = 12.50m
            };

            var seat1 = new Seat
            {
                Id = 1,
                CinemaHall = hall,
                CinemaHallId = hall.Id,
                SeatNumber = "A1",
                SeatType = "Normal"
            };

            var seat2 = new Seat
            {
                Id = 2,
                CinemaHall = hall,
                CinemaHallId = hall.Id,
                SeatNumber = "A2",
                SeatType = "Normal"
            };

            var booking = new Booking
            {
                Id = 10,
                User = user,
                UserId = user.Id,
                Session = session,
                SessionId = session.Id,
                TotalAmount = 25,
                BookingSeats = new List<BookingSeat>
            {
                new() { Seat = seat1, SeatId = seat1.Id, SeatPrice = 12.50m },
                new() { Seat = seat2, SeatId = seat2.Id, SeatPrice = 12.50m }
            }
            };

            ctx.Booking.Add(booking);
            ctx.Payment.Add(new Payment
            {
                BookingId = booking.Id,
                PaymentMethod = "Stripe",
                PaymentStatus = "Pending",
                AmountPaid = 25m,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var fakeSession = new Stripe.Checkout.Session
            {
                Id = "sess_777",
                PaymentStatus = "paid"
            };

            var mockStripe = new Mock<Stripe.Checkout.SessionService>();
            mockStripe.Setup(s => s.Get("sess_777", null, null))
                      .Returns(fakeSession);

            // Define ambiente de teste → bloqueia email/pdf
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
            { "StripeSettings:SecretKey", "sk_test_fake" },
            { "ASPNETCORE_ENVIRONMENT", "Test" }
                })
                .Build();

            var service = new PaymentService(ctx, config, mockStripe.Object);

            // Act
            var result = await service.CheckPaymentStatus("sess_777");

            // Assert
            Assert.Equal("paid", result);
            var payment = ctx.Payment.First();
            Assert.Equal("Paid", payment.PaymentStatus);
        }*/



        #endregion
    }
}
