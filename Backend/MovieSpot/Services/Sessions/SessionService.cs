using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Notifications;

namespace MovieSpot.Services.Sessions
{
    /// <summary>
    /// Provides services for managing cinema sessions in the system,
    /// including creation, update, deletion, and retrieval of session data.
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFcmNotificationService _fcm;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionService"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The database context used to interact with session data.</param>
        public SessionService(ApplicationDbContext context, IFcmNotificationService fcm)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "The database context cannot be null.");
            _fcm = fcm ?? throw new ArgumentNullException(nameof(fcm), "The notification service cannot be null.");
        }

        /// <summary>
        /// Creates a new cinema session.
        /// </summary>
        public Session CreateSession(Session newSession)
        {
            if (newSession == null)
                throw new ArgumentNullException(nameof(newSession), "Session cannot be null.");

            if (newSession.StartDate >= newSession.EndDate)
                throw new ArgumentException("The start date must be earlier than the end date.");

            if (newSession.Price < 0)
                throw new ArgumentException("The session price cannot be negative.");

            if (!_context.Movie.Any(m => m.Id == newSession.MovieId))
                throw new KeyNotFoundException($"Movie with ID {newSession.MovieId} was not found.");

            if (!_context.CinemaHall.Any(h => h.Id == newSession.CinemaHallId))
                throw new KeyNotFoundException($"Cinema hall with ID {newSession.CinemaHallId} was not found.");

            if (!_context.User.Any(u => u.Id == newSession.CreatedBy))
                throw new KeyNotFoundException($"User with ID {newSession.CreatedBy} was not found.");

            bool hasConflict = _context.Session.Any(s =>
                s.CinemaHallId == newSession.CinemaHallId &&
                ((newSession.StartDate >= s.StartDate && newSession.StartDate < s.EndDate) ||
                 (newSession.EndDate > s.StartDate && newSession.EndDate <= s.EndDate)));

            if (hasConflict)
                throw new InvalidOperationException("There is already a session in that time range for this hall.");

            try
            {
                _context.Session.Add(newSession);
                _context.SaveChanges();

                if (newSession.PromotionValue.HasValue && newSession.PromotionValue.Value > 0)
                {
                    var movie = _context.Movie.FirstOrDefault(m => m.Id == newSession.MovieId);
                    var hall = _context.CinemaHall.FirstOrDefault(h => h.Id == newSession.CinemaHallId);

                    var title = "🎟️ Promotional session!";
                    var discount = newSession.PromotionValue.Value;
                    var discountedPrice = newSession.Price * (1 - discount / 100m);

                    var body = $"{movie?.Title ?? "Movie"} — {newSession.StartDate:dd/MM HH:mm} " +
                               $"in hall {hall?.Name ?? newSession.CinemaHallId.ToString()} " +
                               $"with {discount}% discount! Now for {discountedPrice:0.00}€.";

                    _fcm.SendToTopicAsync(
                        topic: "promocoes",
                        title: title,
                        body: body,
                        data: new
                        {
                            sessionId = newSession.Id,
                            movieId = newSession.MovieId,
                            startsAt = newSession.StartDate.ToString("o"),
                            promotion = discount
                        }
                    ).GetAwaiter().GetResult();
                }
                return newSession;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while saving the new session to the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while creating the session.", ex);
            }
        }

        /// <summary>
        /// Deletes a cinema session by its unique identifier.
        /// </summary>
        public Session DeleteSession(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Session ID must be greater than zero.");

            var session = _context.Session.Find(id);
            if (session == null)
                throw new KeyNotFoundException($"Session with ID {id} was not found.");

            try
            {
                _context.Session.Remove(session);
                _context.SaveChanges();
                return session;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while deleting the session from the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while deleting the session.", ex);
            }
        }

        /// <summary>
        /// Retrieves all cinema sessions from the system.
        /// </summary>
        public IEnumerable<Session> GetAllSessions()
        {
            var sessions = _context.Session
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .Include(s => s.CreatedByUser)
                .ToList();

            if (sessions == null || !sessions.Any())
                throw new InvalidOperationException("No sessions are registered in the system.");

            return sessions;
        }

        /// <summary>
        /// Retrieves all available time slots for a given cinema hall and date,
        /// based on the specified movie runtime.
        /// </summary>
        public IEnumerable<TimeSpan> GetAvailableTimes(int cinemaHallId, DateTime date, int runtimeMinutes)
        {
            if (cinemaHallId <= 0)
                throw new ArgumentOutOfRangeException(nameof(cinemaHallId), "Cinema hall ID must be greater than zero.");

            if (runtimeMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(runtimeMinutes), "Runtime must be greater than zero.");

            // Todas as sessões dessa sala nesse dia
            var sessions = _context.Session
                .Where(s => s.CinemaHallId == cinemaHallId && s.StartDate.Date == date.Date)
                .OrderBy(s => s.StartDate)
                .ToList();

            var availableSlots = new List<TimeSpan>();
            var openingTime = new TimeSpan(10, 0, 0);
            var closingTime = new TimeSpan(23, 0, 0);
            var runtime = TimeSpan.FromMinutes(runtimeMinutes);

            var current = openingTime;

            foreach (var session in sessions)
            {
                var busyStart = session.StartDate.TimeOfDay;
                var busyEnd = session.EndDate.TimeOfDay;

                // Gerar todos os slots entre current e início desta sessão
                while (current + runtime <= busyStart)
                {
                    availableSlots.Add(current);
                    // passo = duração do filme (podes pôr TimeSpan.FromMinutes(15) por ex.)
                    current += runtime;
                }

                // Avança current para depois desta sessão
                if (current < busyEnd)
                    current = busyEnd;
            }

            // Depois da última sessão, até ao fecho
            while (current + runtime <= closingTime)
            {
                availableSlots.Add(current);
                current += runtime;
            }

            if (!availableSlots.Any())
                throw new InvalidOperationException("No available time slots exist for this hall and date.");

            return availableSlots;
        }


        /// <summary>
        /// Retrieves a specific cinema session by its unique identifier.
        /// </summary>
        public Session GetSessionById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Session ID must be greater than zero.");

            var session = _context.Session
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .Include(s => s.CreatedByUser)
                .FirstOrDefault(s => s.Id == id);

            if (session == null)
                throw new KeyNotFoundException($"Session with ID {id} was not found.");

            return session;
        }

        /// <summary>
        /// Updates the details of an existing cinema session.
        /// </summary>
        public Session UpdateSession(int id, Session updatedSession)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Session ID must be greater than zero.");

            if (updatedSession == null)
                throw new ArgumentNullException(nameof(updatedSession), "Updated session cannot be null.");

            var session = _context.Session.Find(id);
            if (session == null)
                throw new KeyNotFoundException($"Session with ID {id} was not found.");

            bool hasConflict = _context.Session.Any(s =>
                s.Id != id &&
                s.CinemaHallId == updatedSession.CinemaHallId &&
                ((updatedSession.StartDate >= s.StartDate && updatedSession.StartDate < s.EndDate) ||
                 (updatedSession.EndDate > s.StartDate && updatedSession.EndDate <= s.EndDate)));

            if (hasConflict)
                throw new InvalidOperationException("The new schedule conflicts with another existing session in the same hall.");

            session.MovieId = updatedSession.MovieId;
            session.CinemaHallId = updatedSession.CinemaHallId;
            session.StartDate = updatedSession.StartDate;
            session.EndDate = updatedSession.EndDate;
            session.Price = updatedSession.Price;
            session.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Session.Update(session);
                _context.SaveChanges();
                return session;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DbUpdateConcurrencyException("A concurrency conflict occurred while updating the session.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the session in the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the session.", ex);
            }
        }

        /// <summary>
        /// Retrieves all available seats for a given session,
        /// excluding those already booked.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>A collection of available seats.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the session ID is invalid.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the session does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no seats are available.</exception>
        public IEnumerable<Seat> GetAvailableSeats(int sessionId)
        {
            if (sessionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(sessionId), "Session ID must be greater than zero.");

            var session = _context.Session
                .Include(s => s.CinemaHall)
                .FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                throw new KeyNotFoundException($"Session with ID {sessionId} was not found.");

            var allSeats = _context.Seat
                .Where(seat => seat.CinemaHallId == session.CinemaHallId)
                .ToList();

            var bookedSeats = _context.BookingSeat
                .Include(bs => bs.Booking)
                .Where(bs => bs.Booking.SessionId == sessionId)
                .Select(bs => bs.SeatId)
                .Distinct()
                .ToList();

            var availableSeats = allSeats
                .Where(seat => !bookedSeats.Contains(seat.Id))
                .OrderBy(seat => seat.SeatNumber)
                .ToList();

            if (!availableSeats.Any())
                throw new InvalidOperationException("There are no available seats for this session.");

            return availableSeats;
        }
        public SessionOccupancyDto GetSessionOccupancy(int sessionId)
        {
            var data = _context.Session
                .Where(s => s.Id == sessionId)
                .Select(s => new
                {
                    s.Id,
                    MovieTitle = s.Movie!.Title,
                    HallName = s.CinemaHall!.Name,
                    TotalSeats = s.CinemaHall.Seats.Count(),
                    BookedSeats = s.Bookings.SelectMany(b => b.BookingSeats).Count()
                })
                .FirstOrDefault();

            if (data == null)
                throw new KeyNotFoundException("Sessão não encontrada.");

            return new SessionOccupancyDto
            {
                SessionId = data.Id,
                MovieTitle = data.MovieTitle,
                HallName = data.HallName,
                TotalSeats = data.TotalSeats,
                BookedSeats = data.BookedSeats,
                OccupancyRate = data.TotalSeats > 0
                    ? Math.Round(((double)data.BookedSeats / data.TotalSeats) * 100, 2)
                    : 0
            };
        }
    }
}
