using MovieSpot.Models;

namespace MovieSpot.Services.Sessions
{
    /// <summary>
    /// Defines the contract for managing cinema sessions within the system.
    /// Provides methods for creating, updating, retrieving, and deleting sessions.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Creates a new cinema session.
        /// </summary>
        /// <param name="newSession">The session object containing movie, hall, date, and pricing details.</param>
        /// <returns>The created Session object.</returns>
        Session CreateSession(Session newSession);

        /// <summary>
        /// Updates the details of an existing cinema session.
        /// </summary>
        /// <param name="id">The unique identifier of the session to be updated.</param>
        /// <param name="updatedSession">The session object containing the updated information.</param>
        /// <returns>The updated Session object.</returns>
        Session UpdateSession(int id, Session updatedSession);

        /// <summary>
        /// Deletes a cinema session by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the session to be deleted.</param>
        /// <returns>The deleted Session object.</returns>
        Session DeleteSession(int id);

        /// <summary>
        /// Retrieves a specific cinema session by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the session.</param>
        /// <returns>The Session object associated with the specified ID.</returns>
        Session GetSessionById(int id);

        /// <summary>
        /// Retrieves all cinema sessions in the system.
        /// </summary>
        /// <returns>A collection of all Session objects.</returns>
        IEnumerable<Session> GetAllSessions();

        /// <summary>
        /// Retrieves all available time slots for a given cinema hall and date, 
        /// based on the specified movie runtime.
        /// </summary>
        /// <param name="cinemaHallId">The unique identifier of the cinema hall.</param>
        /// <param name="date">The date for which to check available time slots.</param>
        /// <param name="runtimeMinutes">The duration of the movie in minutes.</param>
        /// <returns>A list of available time slots represented as TimeSpan objects.</returns>
        IEnumerable<TimeSpan> GetAvailableTimes(int cinemaHallId, DateTime date, int runtimeMinutes);

        /// <summary>
        /// Retrieves all available seats for a given session,
        /// excluding those already booked.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>A collection of available Seat objects.</returns>
        IEnumerable<Seat> GetAvailableSeats(int sessionId);

        SessionOccupancyDto GetSessionOccupancy(int sessionId);

    }
}
