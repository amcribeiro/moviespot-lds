using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;

namespace MovieSpot.Services.Cinemas
{
    /// <summary>
    /// Provides services for managing cinemas, including creation,
    /// update, retrieval, and location mapping.
    /// </summary>
    public class CinemaService : ICinemaService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CinemaService"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The database context used to interact with cinema data.</param>
        public CinemaService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all cinemas available in the system.
        /// </summary>
        /// <returns>A list of all registered cinemas.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no cinemas are found.</exception>
        public List<Cinema> GetAllCinemas()
        {
            var cinemas = _context.Cinema
                .Include(c => c.CinemaHalls)
                .AsNoTracking()
                .ToList();

            if (cinemas == null || !cinemas.Any())
                throw new InvalidOperationException("There are no cinemas registered in the system.");

            return cinemas;
        }

        /// <summary>
        /// Retrieves a specific cinema by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema.</param>
        /// <returns>The cinema corresponding to the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the cinema is not found.</exception>
        public Cinema GetCinemaById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The cinema ID must be greater than zero.");

            var cinema = _context.Cinema
                .Include(c => c.CinemaHalls)
                .FirstOrDefault(c => c.Id == id);

            if (cinema == null)
                throw new KeyNotFoundException($"Cinema with ID {id} was not found.");

            return cinema;
        }

        /// <summary>
        /// Adds a new cinema to the system.
        /// </summary>
        /// <param name="cinema">The cinema object containing the data to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided cinema object is null.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving to the database.</exception>
        public void AddCinema(Cinema cinema)
        {
            if (cinema == null)
                throw new ArgumentNullException(nameof(cinema), "The cinema object cannot be null.");

            try
            {
                _context.Cinema.Add(cinema);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while saving the new cinema to the database.", ex);
            }
        }

        /// <summary>
        /// Updates an existing cinema with new information.
        /// </summary>
        /// <param name="cinema">The cinema object containing updated information.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided cinema object is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the cinema does not exist in the system.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while updating the database.</exception>
        public void UpdateCinema(Cinema cinema)
        {
            if (cinema == null)
                throw new ArgumentNullException(nameof(cinema), "The updated cinema object cannot be null.");

            var existing = _context.Cinema.Find(cinema.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Cinema with ID {cinema.Id} was not found.");

            existing.Name = cinema.Name;
            existing.Street = cinema.Street;
            existing.City = cinema.City;
            existing.State = cinema.State;
            existing.ZipCode = cinema.ZipCode;
            existing.Country = cinema.Country;
            existing.Latitude = cinema.Latitude;
            existing.Longitude = cinema.Longitude;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Cinema.Update(existing);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the cinema in the database.", ex);
            }
        }

        /// <summary>
        /// Removes a cinema from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the cinema is not found.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while removing the cinema from the database.</exception>
        public void RemoveCinema(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "The cinema ID must be greater than zero.");

            var cinema = _context.Cinema.Find(id);

            if (cinema == null)
                throw new KeyNotFoundException($"Cinema with ID {id} was not found.");

            try
            {
                _context.Cinema.Remove(cinema);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while removing the cinema from the database.", ex);
            }
        }
    }
}
