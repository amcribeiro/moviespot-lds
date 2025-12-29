using MovieSpot.Models;

namespace MovieSpot.Services.CinemaHalls
{
    public interface ICinemaHallService
    {
        /// <summary>
        /// Retrieves all cinema halls in the system.
        /// </summary>
        /// <returns>An enumerable collection of all cinema halls.</returns>
        IEnumerable<CinemaHall> GetAllCinemaHalls();

        /// <summary>
        /// Retrieves a specific cinema hall by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema hall.</param>
        /// <returns>The cinema hall associated with the specified ID.</returns>
        CinemaHall GetCinemaHallById(int id);

        /// <summary>
        /// Retrieves all cinema halls that belong to a specific cinema.
        /// </summary>
        /// <param name="cinemaId">The unique identifier of the cinema.</param>
        /// <returns>An enumerable collection of cinema halls belonging to the specified cinema.</returns>
        IEnumerable<CinemaHall> GetCinemaHallsByCinemaId(int cinemaId);

        /// <summary>
        /// Creates a new cinema hall in the system.
        /// </summary>
        /// <param name="newCinemaHall">The cinema hall object containing the details for creation.</param>
        /// <returns>The newly created cinema hall object.</returns>
        CinemaHall AddCinemaHall(CinemaHall newCinemaHall);

        /// <summary>
        /// Updates an existing cinema hall with new details.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema hall to be updated.</param>
        /// <param name="updatedCinemaHall">The cinema hall object containing the updated details.</param>
        /// <returns>The updated cinema hall object.</returns>
        CinemaHall UpdateCinemaHall(int id, CinemaHall updatedCinemaHall);

        /// <summary>
        /// Removes a cinema hall by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema hall to be removed.</param>
        void RemoveCinemaHall(int id);
    }
}
