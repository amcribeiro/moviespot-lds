// Services/Movies/IMovieService.cs
using MovieSpot.Models;

namespace MovieSpot.Services.Movies
{
    /// <summary>
    /// Defines the movie-related business operations.
    /// </summary>
    public interface IMovieService
    {
        /// <summary>
        /// Retrieves all movies stored in the database.
        /// </summary>
        /// <returns>A list of <see cref="Movie"/>.</returns>
        List<Movie> GetMovies();

        /// <summary>
        /// Retrieves a single movie by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the movie.</param>
        /// <returns>The <see cref="Movie"/> matching the identifier.</returns>
        Movie GetMovie(int id);

        /// <summary>
        /// Synchronizes movies from the external API into the local database.
        /// </summary>
        Task SyncMovies();

        /// <summary>
        /// Adds a new movie to the database.
        /// </summary>
        /// <param name="movie">The movie entity to add.</param>
        void AddMovie(Movie movie);

        /// <summary>
        /// Removes a movie from the database by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the movie to remove.</param>
        void RemoveMovie(int id);

        List<MovieSessionCountDto> GetSessionCountsPerMovie();

        List<MovieRevenueDto> GetRevenuePerMovie();

        List<PopularMovieDto> GetMostPopularMovies(int count = 5);
    }
}
