using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Services.Bookings
{
    /// <summary>
    /// Provides services for managing bookings in the system,
    /// including creation, update, and retrieval.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFcmNotificationService _fcmService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingService"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The database context used to interact with booking data.</param>
        /// <param name="fcmService">The Firebase Cloud Messaging service used to send push notifications.</param>
        public BookingService(ApplicationDbContext context, IFcmNotificationService fcmService)
        {
            _context = context;
            _fcmService = fcmService;
        }

        /// <summary>
        /// Retrieves all bookings associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose bookings are to be retrieved.</param>
        /// <returns>A collection of bookings belonging to the specified user.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the user ID is less than or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no bookings are found for the user.</exception>
        public IEnumerable<Booking> GetAllBookingsByUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "The user ID must be greater than zero.");

            var bookings = _context.Booking
                .Where(b => b.UserId == userId)
                .ToList();

            if (bookings == null || !bookings.Any())
                throw new InvalidOperationException("No bookings were found for this user.");

            return bookings;
        }

        /// <summary>
        /// Retrieves all bookings in the system.
        /// </summary>
        /// <returns>A collection of all bookings.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no bookings are found.</exception>
        public IEnumerable<Booking> GetAllBookings()
        {
            var bookings = _context.Booking.ToList();

            if (bookings == null || !bookings.Any())
                throw new InvalidOperationException("There are no bookings registered in the system.");

            return bookings;
        }

        /// <summary>
        /// Retrieves a specific booking by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the booking.</param>
        /// <returns>The booking associated with the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the booking is not found.</exception>
        public Booking GetBookingById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The booking ID must be greater than zero.");

            var booking = _context.Booking.Find(id);

            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {id} was not found.");

            return booking;
        }

        /// <summary>
        /// Creates a new booking in the system.
        /// </summary>
        /// <param name="newBooking">The booking object containing the details for creation.</param>
        /// <returns>The newly created booking object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided booking object is null.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving changes to the database.</exception>
        public Booking CreateBooking(Booking newBooking)
        {
            if (newBooking == null)
                throw new ArgumentNullException(nameof(newBooking), "The booking cannot be null.");

            try
            {
                _context.Booking.Add(newBooking);
                _context.SaveChanges();
                return newBooking;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while saving the new booking to the database.", ex);
            }
        }

        /// <summary>
        /// Updates an existing booking with new details.
        /// </summary>
        /// <param name="id">The unique identifier of the booking to be updated.</param>
        /// <param name="updatedBooking">The booking object containing the updated details.</param>
        /// <returns>The updated booking object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the updated booking object is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the booking is not found.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving changes to the database.</exception>
        public Booking UpdateBooking(int id, Booking updatedBooking)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The booking ID must be greater than zero.");

            if (updatedBooking == null)
                throw new ArgumentNullException(nameof(updatedBooking), "The updated booking cannot be null.");

            var booking = _context.Booking.Find(id);

            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {id} was not found.");

            booking.UserId = updatedBooking.UserId;
            booking.SessionId = updatedBooking.SessionId;
            booking.Status = updatedBooking.Status;
            booking.TotalAmount = updatedBooking.TotalAmount;
            booking.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Booking.Update(booking);
                _context.SaveChanges();
                return booking;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the booking in the database.", ex);
            }
        }

        /// <summary>
        /// Creates a new booking and associates it with the specified seats,
        /// calculating the total amount based on seat type, session price and promotions.
        /// </summary>
        /// <param name="newBooking">The booking to create.</param>
        /// <param name="seatIds">The collection of seat IDs to be reserved for this booking.</param>
        /// <returns>The created booking with the calculated total amount.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the booking object is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the seat list is null or empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the session is invalid, one or more seats do not exist,
        /// or one or more seats are already reserved for this session.
        /// </exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving booking data.</exception>
        public Booking CreateBookingWithSeats(Booking newBooking, IEnumerable<int> seatIds)
        {
            if (newBooking == null)
                throw new ArgumentNullException(nameof(newBooking), "The booking cannot be null.");

            if (seatIds == null || !seatIds.Any())
                throw new ArgumentException("The seat list cannot be empty.", nameof(seatIds));

            var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
            using var tx = isInMemory ? null : _context.Database.BeginTransaction();

            try
            {
                var session = _context.Session.FirstOrDefault(s => s.Id == newBooking.SessionId);
                if (session == null)
                    throw new InvalidOperationException("Invalid session.");

                var seats = _context.Seat
                    .Where(s => seatIds.Contains(s.Id))
                    .ToList();

                if (seats.Count != seatIds.Count())
                    throw new InvalidOperationException("One or more seats do not exist.");

                bool anyTaken = _context.BookingSeat
                    .Include(bs => bs.Booking)
                    .Any(bs => seatIds.Contains(bs.SeatId) && bs.Booking.SessionId == newBooking.SessionId);

                if (anyTaken)
                    throw new InvalidOperationException("One or more seats are already reserved for this session.");

                decimal total = 0;
                var bookingSeats = new List<BookingSeat>();

                foreach (var seat in seats)
                {
                    decimal seatPrice = session.Price;

                    if (seat.SeatType.Equals("VIP", StringComparison.OrdinalIgnoreCase))
                        seatPrice *= 1.5m;
                    else if (seat.SeatType.Equals("Reduced", StringComparison.OrdinalIgnoreCase))
                        seatPrice *= 1.25m;

                    if (session.PromotionValue.HasValue && session.PromotionValue.Value > 0)
                        seatPrice -= seatPrice * (session.PromotionValue.Value / 100m);

                    total += seatPrice;

                    bookingSeats.Add(new BookingSeat
                    {
                        SeatId = seat.Id,
                        SeatPrice = seatPrice,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                newBooking.TotalAmount = total;

                _context.Booking.Add(newBooking);
                _context.SaveChanges();

                foreach (var bs in bookingSeats)
                    bs.BookingId = newBooking.Id;

                _context.BookingSeat.AddRange(bookingSeats);
                _context.SaveChanges();

                if (!isInMemory)
                    tx?.Commit();

                return newBooking;
            }
            catch (DbUpdateException ex)
            {
                if (!isInMemory)
                    tx?.Rollback();
                throw new DbUpdateException("An error occurred while saving the new booking to the database.", ex);
            }
            catch
            {
                if (!isInMemory)
                    tx?.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Checks all bookings with sessions scheduled for the next day
        /// and sends push notifications to the corresponding users.
        /// </summary>
        /// <returns>The total number of notifications sent.</returns>
        public async Task<int> SendDailyRemindersAsync()
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            var bookings = await _context.Booking
                .Include(b => b.User)
                .Include(b => b.Session)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Session)
                    .ThenInclude(s => s.CinemaHall)
                .Where(b => b.Status == true &&
                            b.Session != null &&
                            b.Session.StartDate.Date == tomorrow)
                .ToListAsync();

            if (!bookings.Any())
                return 0;

            int notificationsSent = 0;

            foreach (var booking in bookings)
            {
                var user = booking.User;
                var session = booking.Session;
                var movie = session?.Movie;
                var hall = session?.CinemaHall;

                if (user == null || session == null)
                    continue;

                var title = "Reminder: you have a session tomorrow!";
                var body = $"{movie?.Title ?? "Movie"} — {session.StartDate:HH:mm} in hall {hall?.Name ?? session.CinemaHallId.ToString()}.";

                try
                {
                    await _fcmService.SendToTopicAsync(
                        topic: $"user_{user.Id}",
                        title: title,
                        body: body,
                        data: new
                        {
                            bookingId = booking.Id,
                            sessionId = session.Id,
                            startsAt = session.StartDate.ToString("o")
                        }
                    );

                    notificationsSent++;
                }
                catch
                {
                    continue;
                }
            }

            return notificationsSent;
        }

        /// <summary>
        /// Procura bookings criados há mais de 15 minutos que ainda não foram confirmados,
        /// marca pagamentos pendentes como Expired e remove os BookingSeats.
        /// </summary>
        public async Task CleanupExpiredBookingsAsync()
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-15);

            // 🔎 Bookings que já passaram o tempo e ainda não estão confirmados
            var expiredBookings = await _context.Booking
                .Include(b => b.BookingSeats)
                .Where(b => b.CreatedAt <= cutoff && b.Status == false)
                .ToListAsync();

            if (!expiredBookings.Any())
                return;

            foreach (var booking in expiredBookings)
            {
                // ⚠️ Pagamentos pendentes deste booking → Expired
                var pendingPayments = await _context.Payment
                    .Where(p => p.BookingId == booking.Id && p.PaymentStatus == "Pending")
                    .ToListAsync();

                foreach (var payment in pendingPayments)
                {
                    payment.PaymentStatus = "Expired";
                    payment.UpdatedAt = now;
                    _context.Payment.Update(payment);
                }

                // 💺 Liberta os lugares desta reserva
                if (booking.BookingSeats.Any())
                {
                    _context.BookingSeat.RemoveRange(booking.BookingSeats);
                }

                // (Opcional) marca booking como expirado de forma mais explícita
                booking.UpdatedAt = now;
                // booking.Status = false; // já está falso, mas podes deixar assim para garantir

                _context.Booking.Update(booking);
            }

            await _context.SaveChangesAsync();
        }

        public List<PeakHourDto> GetPeakBookingHours()
        {
            return _context.Booking
                .GroupBy(b => b.CreatedAt.Hour) // Agrupa pela hora da criação da reserva
                .Select(g => new PeakHourDto
                {
                    HourOfDay = g.Key,
                    BookingsMade = g.Count()
                })
                .OrderByDescending(x => x.BookingsMade) // As horas mais movimentadas primeiro
                .ToList();
        }
    }
}
