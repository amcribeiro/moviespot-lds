using MovieSpot.DTO_s;
using MovieSpot.Models;

namespace MovieSpot.Services.Seats
{
    /// <summary>
    /// Business rules and operations related to cinema seats (Seats) within cinema halls.
    /// </summary>
    public interface ISeatService
    {
        /// <summary>
        /// Retrieves all seats available in the system.
        /// </summary>
        /// <returns>A list of all <see cref="Seat"/> objects.</returns>
        Task<List<Seat>> GetAllSeatsAsync();

        /// <summary>
        /// Retrieves a seat by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the seat to retrieve.</param>
        /// <returns>The matching <see cref="Seat"/> if found; otherwise, null.</returns>
        Task<Seat?> GetSeatByIdAsync(int id);

        /// <summary>
        /// Retrieves all seats that belong to a specific cinema hall.
        /// </summary>
        /// <param name="cinemaHallId">The ID of the cinema hall.</param>
        /// <returns>A list of seats associated with the given cinema hall.</returns>
        Task<List<Seat>> GetSeatsByCinemaHallIdAsync(int cinemaHallId);

        /// <summary>
        /// Creates a new seat within a specified cinema hall.  
        /// Ensures that the seat number is unique within that hall.
        /// </summary>
        /// <param name="seat">The seat object to create.</param>
        /// <returns>The newly created <see cref="Seat"/>.</returns>
        Task<Seat> AddSeatAsync(Seat seat);

        /// <summary>
        /// Updates an existing seat record.  
        /// Keeps the original creation timestamp and refreshes the update timestamp.
        /// </summary>
        /// <param name="seat">The seat object containing updated information.</param>
        /// <returns>The updated <see cref="Seat"/>.</returns>
        Task<Seat> UpdateSeatAsync(Seat seat);

        /// <summary>
        /// Removes a seat from the system using its ID.  
        /// Returns <c>true</c> if the seat was removed successfully,  
        /// or <c>false</c> if it did not exist.
        /// </summary>
        /// <param name="id">The ID of the seat to remove.</param>
        /// <returns>A boolean indicating whether the seat was successfully removed.</returns>
        Task<bool> RemoveSeatAsync(int id);

        Task<SeatDTO.SeatResponsePriceDto> GetSeatPriceAsync(int seatId, int sessionId);

    }
}
