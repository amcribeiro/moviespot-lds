using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;

namespace MovieSpot.Services.CinemaHalls
{
    /// <summary>
    /// Provides services for managing cinema halls,
    /// including creation, update, and retrieval of data.
    /// </summary>
    public class CinemaHallService : ICinemaHallService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CinemaHallService"/> class
        /// with the specified database context.
        /// </summary>
        /// <param name="context">The database context used to interact with cinema hall data.</param>
        public CinemaHallService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all cinema halls registered in the system.
        /// </summary>
        /// <returns>A collection of all cinema halls.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no cinema halls are registered.</exception>
        public IEnumerable<CinemaHall> GetAllCinemaHalls()
        {
            var halls = _context.CinemaHall
                .Include(ch => ch.Cinema)
                .ToList();

            if (halls == null || !halls.Any())
                throw new InvalidOperationException("There are no cinema halls registered in the system.");

            return halls;
        }

        /// <summary>
        /// Retrieves a cinema hall based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema hall.</param>
        /// <returns>The cinema hall that matches the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the cinema hall is not found.</exception>
        public CinemaHall GetCinemaHallById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The cinema hall ID must be greater than zero.");

            var hall = _context.CinemaHall
                .Include(ch => ch.Cinema)
                .FirstOrDefault(ch => ch.Id == id);

            if (hall == null)
                throw new KeyNotFoundException($"Cinema hall with ID {id} was not found.");

            return hall;
        }

        /// <summary>
        /// Retrieves all cinema halls associated with a specific cinema.
        /// </summary>
        /// <param name="cinemaId">The unique identifier of the cinema.</param>
        /// <returns>A collection of cinema halls belonging to the specified cinema.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the cinema ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no halls are found for the specified cinema.</exception>
        public IEnumerable<CinemaHall> GetCinemaHallsByCinemaId(int cinemaId)
        {
            if (cinemaId <= 0)
                throw new ArgumentOutOfRangeException(nameof(cinemaId), "The cinema ID must be greater than zero.");

            var halls = _context.CinemaHall
                .Include(ch => ch.Cinema)
                .Where(ch => ch.CinemaId == cinemaId)
                .ToList();

            if (!halls.Any())
                throw new KeyNotFoundException($"No cinema halls were found for the cinema with ID {cinemaId}.");

            return halls;
        }

        /// <summary>
        /// Creates a new cinema hall in the system.
        /// </summary>
        /// <param name="newCinemaHall">The object containing the details of the cinema hall to be created.</param>
        /// <returns>The created cinema hall.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the cinema hall object is null.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving the cinema hall to the database.</exception>
        public CinemaHall AddCinemaHall(CinemaHall newCinemaHall)
        {
            if (newCinemaHall == null)
                throw new ArgumentNullException(nameof(newCinemaHall), "The cinema hall cannot be null.");

            try
            {
                _context.CinemaHall.Add(newCinemaHall);
                _context.SaveChanges();
                return newCinemaHall;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while saving the new cinema hall to the database.", ex);
            }
        }

        /// <summary>
        /// Updates an existing cinema hall with new data.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema hall to update.</param>
        /// <param name="updatedCinemaHall">The object containing the updated cinema hall data.</param>
        /// <returns>The updated cinema hall.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the updated cinema hall object is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the cinema hall is not found.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while updating the database.</exception>
        public CinemaHall UpdateCinemaHall(int id, CinemaHall updatedCinemaHall)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The cinema hall ID must be greater than zero.");

            if (updatedCinemaHall == null)
                throw new ArgumentNullException(nameof(updatedCinemaHall), "The updated cinema hall cannot be null.");

            var existing = _context.CinemaHall.Find(id);

            if (existing == null)
                throw new KeyNotFoundException($"Cinema hall with ID {id} was not found.");

            existing.Name = updatedCinemaHall.Name;
            existing.CinemaId = updatedCinemaHall.CinemaId;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.CinemaHall.Update(existing);
                _context.SaveChanges();
                return existing;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the cinema hall in the database.", ex);
            }
        }

        /// <summary>
        /// Removes a cinema hall by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the cinema hall to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the cinema hall is not found.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while deleting the cinema hall from the database.</exception>
        public void RemoveCinemaHall(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The cinema hall ID must be greater than zero.");

            var hall = _context.CinemaHall.Find(id);

            if (hall == null)
                throw new KeyNotFoundException($"Cinema hall with ID {id} was not found.");

            try
            {
                _context.CinemaHall.Remove(hall);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while removing the cinema hall from the database.", ex);
            }
        }
    }
}
