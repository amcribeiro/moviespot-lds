using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.DTO_s;
using MovieSpot.Models;

namespace MovieSpot.Services.Seats
{
    /// <summary>
    /// Implementation of seat-related operations using EF Core.
    /// Context: online cinema management (cinemas → halls → seats).
    /// </summary>
    public class SeatService : ISeatService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeatService"/> class
        /// with the specified database context.
        /// </summary>
        /// <param name="context">The database context used for accessing seat data.</param>
        public SeatService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves all seats in the system, ordered by cinema hall and seat number.
        /// </summary>
        /// <returns>A list of all seats.</returns>
        public async Task<List<Seat>> GetAllSeatsAsync()
        {
            return await _context.Seat
                .AsNoTracking()
                .OrderBy(s => s.CinemaHallId).ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a seat by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the seat.</param>
        /// <returns>The seat if found; otherwise, <c>null</c>.</returns>
        public async Task<Seat?> GetSeatByIdAsync(int id)
        {
            return await _context.Seat
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves all seats for a specific cinema hall.
        /// </summary>
        /// <param name="cinemaHallId">The unique identifier of the cinema hall.</param>
        /// <returns>A list of seats belonging to the specified cinema hall.</returns>
        public async Task<List<Seat>> GetSeatsByCinemaHallIdAsync(int cinemaHallId)
        {
            return await _context.Seat
                .AsNoTracking()
                .Where(s => s.CinemaHallId == cinemaHallId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new seat to the database after validating its cinema hall and uniqueness.
        /// </summary>
        /// <param name="seat">The seat to add.</param>
        /// <returns>The newly created seat.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the seat is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the cinema hall does not exist or a seat with the same number already exists in the hall.
        /// </exception>
        public async Task<Seat> AddSeatAsync(Seat seat)
        {
            if (seat is null)
                throw new ArgumentNullException(nameof(seat));

            await EnsureCinemaHallExists(seat.CinemaHallId);
            await EnsureSeatNumberUniqueInHall(seat.CinemaHallId, seat.SeatNumber);

            seat.CreatedAt = DateTime.UtcNow;
            seat.UpdatedAt = seat.CreatedAt;

            _context.Seat.Add(seat);
            await _context.SaveChangesAsync();

            return seat;
        }

        /// <summary>
        /// Updates an existing seat with new data.
        /// </summary>
        /// <param name="seat">The updated seat data.</param>
        /// <returns>The updated seat.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the seat is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the seat does not exist.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the cinema hall does not exist or another seat with the same number already exists.
        /// </exception>
        public async Task<Seat> UpdateSeatAsync(Seat seat)
        {
            if (seat is null)
                throw new ArgumentNullException(nameof(seat));

            var existing = await _context.Seat
                .FirstOrDefaultAsync(s => s.Id == seat.Id);

            if (existing is null)
                throw new KeyNotFoundException($"Seat #{seat.Id} was not found.");

            var hallChanged = existing.CinemaHallId != seat.CinemaHallId;
            var numberChanged = !string.Equals(existing.SeatNumber, seat.SeatNumber, StringComparison.Ordinal);

            if (hallChanged)
                await EnsureCinemaHallExists(seat.CinemaHallId);

            if (hallChanged || numberChanged)
                await EnsureSeatNumberUniqueInHall(seat.CinemaHallId, seat.SeatNumber, ignoreSeatId: seat.Id);

            existing.CinemaHallId = seat.CinemaHallId;
            existing.SeatNumber = seat.SeatNumber;
            existing.SeatType = seat.SeatType;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Removes a seat by its unique identifier.
        /// </summary>
        /// <param name="id">The seat ID to remove.</param>
        /// <returns><c>true</c> if the seat was successfully removed; otherwise, <c>false</c>.</returns>
        public async Task<bool> RemoveSeatAsync(int id)
        {
            var existing = await _context.Seat.FirstOrDefaultAsync(s => s.Id == id);
            if (existing is null)
                return false;

            _context.Seat.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SeatDTO.SeatResponsePriceDto?> GetSeatPriceAsync(int seatId, int sessionId)
        {
            var seat = await _context.Seat
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == seatId);

            if (seat == null)
                return null;

            var session = await _context.Session
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return null;

            decimal price = seat.SeatType?.ToLower() switch
            {
                "vip" => session.Price * 1.5m,
                "reduced" => session.Price * 1.25m,
                _ => session.Price
            };

            return new SeatDTO.SeatResponsePriceDto
            {
                Id = seat.Id,
                CinemaHallId = seat.CinemaHallId,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType,
                Price = (double)Math.Round(price, 2),
                CreatedAt = seat.CreatedAt,
                UpdatedAt = seat.UpdatedAt
            };
        }

        /// <summary>
        /// Ensures that the specified cinema hall exists in the database.
        /// </summary>
        /// <param name="cinemaHallId">The cinema hall ID to verify.</param>
        /// <exception cref="InvalidOperationException">Thrown when the cinema hall does not exist.</exception>
        private async Task EnsureCinemaHallExists(int cinemaHallId)
        {
            var exists = await _context.CinemaHall
                .AsNoTracking()
                .AnyAsync(ch => ch.Id == cinemaHallId);

            if (!exists)
                throw new InvalidOperationException($"CinemaHall #{cinemaHallId} does not exist.");
        }

        /// <summary>
        /// Ensures that the seat number is unique within a specific cinema hall.
        /// </summary>
        /// <param name="cinemaHallId">The cinema hall ID.</param>
        /// <param name="seatNumber">The seat number to validate.</param>
        /// <param name="ignoreSeatId">Optional seat ID to ignore during validation (useful during updates).</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a seat with the same number already exists in the specified cinema hall.
        /// </exception>
        private async Task EnsureSeatNumberUniqueInHall(int cinemaHallId, string seatNumber, int? ignoreSeatId = null)
        {
            var query = _context.Seat.AsNoTracking()
                .Where(s => s.CinemaHallId == cinemaHallId && s.SeatNumber == seatNumber);

            if (ignoreSeatId.HasValue)
                query = query.Where(s => s.Id != ignoreSeatId.Value);

            var exists = await query.AnyAsync();
            if (exists)
                throw new InvalidOperationException($"A seat with number '{seatNumber}' already exists in hall #{cinemaHallId}.");
        }
    }
}
