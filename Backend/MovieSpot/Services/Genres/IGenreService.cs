using MovieSpot.Models;

namespace MovieSpot.Services.Genres
{
    /// <summary>
    /// Defines methods for managing and synchronizing genres between
    /// the local database and an external movie API.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Synchronizes all genres from the external API into the local database.
        /// If new genres are found in the API, they are added to the database.
        /// </summary>
        /// <remarks>
        /// This method should be used to keep the local genre repository up to date
        /// with the external movie API (e.g., TMDb).
        /// </remarks>
        /// <exception cref="HttpRequestException">
        /// Thrown when there is an issue connecting to the external API.
        /// </exception>
        /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">
        /// Thrown when saving genres to the database fails.
        /// </exception>
        Task SyncGenresAsync();

        /// <summary>
        /// Retrieves all genres available from the external movie API.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.  
        /// The task result contains a list of <see cref="Genre"/> objects retrieved from the external API.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the external API request fails or returns an invalid response.
        /// </exception>
        Task<List<Genre>> GetAllGenresFromApiAsync();


        /// <summary>
        /// Retrieves all genres stored in the local database.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Genre"/> objects from the local database.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when no genres are found in the database.</exception>
        List<Genre> GetAllGenresFromDb();

        /// <summary>
        /// Retrieves a specific genre by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the genre to retrieve.</param>
        /// <returns>
        /// The <see cref="Genre"/> object that matches the specified ID.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the genre ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no genre is found with the specified ID.</exception>
        Genre GetGenreById(int id);

        List<GenreStatDto> GetGenreStatistics();
    }
}
