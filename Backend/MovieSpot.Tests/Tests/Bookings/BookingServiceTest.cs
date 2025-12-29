using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Bookings;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Tests.Services.Bookings
{
    public class BookingServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IFcmNotificationService> _fcmMock;
        private readonly BookingService _service;

        public BookingServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _fcmMock = new Mock<IFcmNotificationService>();
            _service = new BookingService(_context, _fcmMock.Object);
        }

        private void SeedData()
        {
            _context.Booking.AddRange(new List<Booking>
            {
                new Booking { Id = 1, UserId = 10, SessionId = 100, TotalAmount = 50m },
                new Booking { Id = 2, UserId = 20, SessionId = 101, TotalAmount = 75m },
                new Booking { Id = 3, UserId = 10, SessionId = 102, TotalAmount = 120m }
            });
            _context.SaveChanges();
        }

        #region GetAllBookingsByUserId

        [Fact]
        public void GetAllBookingsByUserId_ValidUserId_ReturnsBookings()
        {
            SeedData();

            var result = _service.GetAllBookingsByUserId(10);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Equal(10, b.UserId));
        }

        [Fact]
        public void GetAllBookingsByUserId_InvalidUserId_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetAllBookingsByUserId(0));
        }

        [Fact]
        public void GetAllBookingsByUserId_NoBookings_ThrowsInvalidOperationException()
        {
            SeedData();
            Assert.Throws<InvalidOperationException>(() => _service.GetAllBookingsByUserId(999));
        }

        #endregion

        #region GetAllBookings

        [Fact]
        public void GetAllBookings_WhenBookingsExist_ReturnsAll()
        {
            SeedData();
            var result = _service.GetAllBookings();
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void GetAllBookings_WhenEmpty_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _service.GetAllBookings());
        }

        #endregion

        #region GetBookingById

        [Fact]
        public void GetBookingById_ValidId_ReturnsBooking()
        {
            SeedData();
            var result = _service.GetBookingById(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetBookingById_InvalidId_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetBookingById(0));
        }

        [Fact]
        public void GetBookingById_NotFound_ThrowsKeyNotFoundException()
        {
            SeedData();
            Assert.Throws<KeyNotFoundException>(() => _service.GetBookingById(999));
        }

        #endregion

        #region CreateBooking

        [Fact]
        public void CreateBooking_ValidBooking_ReturnsCreatedBooking()
        {
            var booking = new Booking { UserId = 30, SessionId = 103 };
            var result = _service.CreateBooking(booking);

            Assert.NotNull(result);
            Assert.Equal(booking.UserId, result.UserId);
            Assert.Equal(1, _context.Booking.Count());
        }

        [Fact]
        public void CreateBooking_NullBooking_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.CreateBooking(null));
        }

        [Fact]
        public void CreateBooking_DbUpdateFails_ThrowsDbUpdateException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new FailingDbContext(options);

            var service = new BookingService(context, _fcmMock.Object);
            var booking = new Booking { UserId = 1, SessionId = 1 };

            Assert.Throws<DbUpdateException>(() => service.CreateBooking(booking));
        }

        private class FailingDbContext : ApplicationDbContext
        {
            public FailingDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            public override int SaveChanges()
            {
                throw new DbUpdateException("Simulated failure");
            }
        }

        #endregion

        #region UpdateBooking

        [Fact]
        public void UpdateBooking_ValidData_UpdatesBooking()
        {
            SeedData();

            var updated = new Booking
            {
                UserId = 50,
                SessionId = 200,
                TotalAmount = 250m,
                Status = false
            };

            var result = _service.UpdateBooking(1, updated);

            Assert.Equal(50, result.UserId);
            Assert.Equal(200, result.SessionId);
            Assert.Equal(250m, result.TotalAmount);
            Assert.False(result.Status);
        }

        [Fact]
        public void UpdateBooking_IdLessOrEqualZero_ThrowsArgumentOutOfRangeException()
        {
            var booking = new Booking { UserId = 1, SessionId = 1 };
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateBooking(0, booking));
        }

        [Fact]
        public void UpdateBooking_NullBooking_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.UpdateBooking(1, null));
        }

        [Fact]
        public void UpdateBooking_NotFound_ThrowsKeyNotFoundException()
        {
            var booking = new Booking { UserId = 1, SessionId = 1 };
            Assert.Throws<KeyNotFoundException>(() => _service.UpdateBooking(999, booking));
        }
        [Fact]
        public void UpdateBooking_DbUpdateFails_ThrowsDbUpdateException()
        {
            var seedOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var normalContext = new ApplicationDbContext(options))
            {
                normalContext.Booking.Add(new Booking { Id = 1, UserId = 10, SessionId = 100, TotalAmount = 50m });
                normalContext.SaveChanges();
            }

            var failingOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var failingContext = new FailingDbContext(failingOptions);
            var service = new BookingService(failingContext, _fcmMock.Object);

            var updated = new Booking { UserId = 5, SessionId = 200, TotalAmount = 99m };

            Assert.Throws<DbUpdateException>(() => service.UpdateBooking(1, updated));
        }



        #endregion

        #region CreateBookingWithSeats

        [Fact]
        public void CreateBookingWithSeats_ValidData_CreatesBookingAndSeats()
        {
            var session = new Session { Id = 1, Price = 10.0m };
            _context.Session.Add(session);
            var seats = new List<Seat>
            {
                new Seat { Id = 1, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Normal" },
                new Seat { Id = 2, CinemaHallId = 1, SeatNumber = "A2", SeatType = "VIP" }
            };
            _context.Seat.AddRange(seats);
            _context.SaveChanges();

            var booking = new Booking { UserId = 1, SessionId = 1 };
            var result = _service.CreateBookingWithSeats(booking, new List<int> { 1, 2 });

            Assert.NotNull(result);
            Assert.Equal(2, _context.BookingSeat.Count());
            var total = _context.BookingSeat.Sum(bs => bs.SeatPrice);
            Assert.Equal(total, result.TotalAmount);
        }

        [Fact]
        public void CreateBookingWithSeats_NullBooking_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.CreateBookingWithSeats(null, new List<int> { 1 }));
        }

        [Fact]
        public void CreateBookingWithSeats_EmptySeats_ThrowsArgumentException()
        {
            var booking = new Booking { UserId = 1, SessionId = 1 };
            Assert.Throws<ArgumentException>(() => _service.CreateBookingWithSeats(booking, new List<int>()));
        }

        [Fact]
        public void CreateBookingWithSeats_InvalidSession_ThrowsInvalidOperationException()
        {
            var booking = new Booking { UserId = 1, SessionId = 999 };
            Assert.Throws<InvalidOperationException>(() => _service.CreateBookingWithSeats(booking, new List<int> { 1 }));
        }

        [Fact]
        public void CreateBookingWithSeats_SeatNotFound_ThrowsInvalidOperationException()
        {
            var session = new Session { Id = 1, Price = 10m };
            _context.Session.Add(session);
            _context.Seat.Add(new Seat { Id = 99, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Normal" });
            _context.SaveChanges();

            var booking = new Booking { UserId = 1, SessionId = 1 };
            Assert.Throws<InvalidOperationException>(() => _service.CreateBookingWithSeats(booking, new List<int> { 1 }));
        }

        [Fact]
        public void CreateBookingWithSeats_SeatAlreadyReserved_ThrowsInvalidOperationException()
        {
            var session = new Session { Id = 1, Price = 10.0m };
            var seat = new Seat { Id = 1, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Normal" };
            _context.Session.Add(session);
            _context.Seat.Add(seat);
            _context.SaveChanges();

            var existingBooking = new Booking { Id = 1, UserId = 2, SessionId = 1 };
            _context.Booking.Add(existingBooking);
            _context.BookingSeat.Add(new BookingSeat { BookingId = 1, SeatId = 1, SeatPrice = 10.0m });
            _context.SaveChanges();

            var newBooking = new Booking { UserId = 3, SessionId = 1 };
            Assert.Throws<InvalidOperationException>(() => _service.CreateBookingWithSeats(newBooking, new List<int> { 1 }));
        }

        [Fact]
        public void CreateBookingWithSeats_WithPromotionAndReducedSeat_CalculatesDiscountedTotal()
        {
            var session = new Session { Id = 1, Price = 20m, PromotionValue = 50 };
            _context.Session.Add(session);
            var seat = new Seat { Id = 1, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Reduced" };
            _context.Seat.Add(seat);
            _context.SaveChanges();

            var booking = new Booking { UserId = 1, SessionId = 1 };
            var result = _service.CreateBookingWithSeats(booking, new List<int> { 1 });

            Assert.Equal(12.5m, result.TotalAmount);
        }

        [Fact]
        public void CreateBookingWithSeats_DbUpdateFails_ThrowsDbUpdateException()
        {
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
            { CallBase = true };

            mockContext.Object.Session.Add(new Session { Id = 1, Price = 10m });
            mockContext.Object.Seat.Add(new Seat { Id = 1, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Normal" });
            mockContext.Object.SaveChanges();

            mockContext.Setup(c => c.SaveChanges()).Throws(new DbUpdateException("Erro BD"));

            var service = new BookingService(mockContext.Object, _fcmMock.Object);
            var booking = new Booking { UserId = 1, SessionId = 1 };

            Assert.Throws<DbUpdateException>(() => service.CreateBookingWithSeats(booking, new List<int> { 1 }));
        }

        #endregion

        #region SendDailyRemindersAsync

        [Fact]
        public async Task SendDailyRemindersAsync_NoBookings_ReturnsZero()
        {
            var result = await _service.SendDailyRemindersAsync();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task SendDailyRemindersAsync_BookingsForTomorrow_SendsNotification()
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var user = new User { Id = 1, Name = "John Doe" };
            var movie = new Movie { Id = 1, Title = "Matrix" };
            var hall = new CinemaHall { Id = 1, Name = "Sala 1" };
            var session = new Session
            {
                Id = 1,
                MovieId = 1,
                Movie = movie,
                CinemaHallId = 1,
                CinemaHall = hall,
                StartDate = tomorrow.AddHours(18),
                Price = 10m
            };
            var booking = new Booking
            {
                Id = 1,
                UserId = 1,
                User = user,
                SessionId = 1,
                Session = session,
                Status = true
            };

            _context.AddRange(user, movie, hall, session, booking);
            _context.SaveChanges();

            _fcmMock.Setup(x => x.SendToTopicAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            var result = await _service.SendDailyRemindersAsync();

            Assert.Equal(1, result);
            _fcmMock.Verify(x => x.SendToTopicAsync("user_1", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task SendDailyRemindersAsync_NotificationThrowsException_ContinuesExecution()
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var user = new User { Id = 1, Name = "Jane" };
            var session = new Session
            {
                Id = 1,
                StartDate = tomorrow.AddHours(18),
                Price = 10m,
                Movie = new Movie { Id = 1, Title = "Titanic" },
                CinemaHall = new CinemaHall { Id = 1, Name = "Sala 2" }
            };
            var booking = new Booking
            {
                Id = 1,
                User = user,
                UserId = 1,
                Session = session,
                SessionId = 1,
                Status = true
            };

            _context.AddRange(user, session, session.Movie, session.CinemaHall, booking);
            _context.SaveChanges();

            _fcmMock.Setup(x => x.SendToTopicAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("Falha FCM"));

            var result = await _service.SendDailyRemindersAsync();
            Assert.Equal(0, result);
        }

        #endregion
    }
}
