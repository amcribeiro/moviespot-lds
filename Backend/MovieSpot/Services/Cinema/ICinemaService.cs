using MovieSpot.Models;

namespace MovieSpot.Services.Cinemas
{
    /// <summary>
    /// Defines the contract for cinema-related business operations.
    /// </summary>
    public interface ICinemaService
    {
        /// <summary>
        /// Retrieves all cinemas from the system.
        /// </summary>
        /// <returns>A list of all registered cinemas.</returns>
        List<Cinema> GetAllCinemas();

        /// <summary>
        /// Retrieves a cinema by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema.</param>
        /// <returns>The cinema associated with the specified ID.</returns>
        Cinema GetCinemaById(int id);


        /// <summary>
        /// Adds a new cinema to the system.
        /// </summary>
        /// <param name="cinema">The cinema entity to add.</param>
        void AddCinema(Cinema cinema);

        /// <summary>
        /// Updates an existing cinema with new information.
        /// </summary>
        /// <param name="cinema">The updated cinema entity.</param>
        void UpdateCinema(Cinema cinema);

        /// <summary>
        /// Removes a cinema from the system by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema to remove.</param>
        void RemoveCinema(int id);
    }
}
