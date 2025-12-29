using MovieSpot.Models;

namespace MovieSpot.Services.Movies
{
    /// <summary>
    /// External TMDB API abstraction used by MovieService.
    /// </summary>
    public interface ITMDBAPIService
    {
        /// <summary>
        /// Gets trending movies for a given time window ("day" or "week").
        /// </summary>
        Task<List<int>> GetTrendingMovies(string timeWindow);

        /// <summary>
        /// Gets detailed information of a single movie from TMDB.
        /// </summary>
        Task<MovieFromAPI?> GetMovieFromAPI(int id);

        /// <summary>
        /// Gets a list of trending movies from TMDB and fetches their full details.
        /// </summary>
        /// <param name="timeWindow">
        /// The time range for trending movies (<c>"day"</c> or <c>"week"</c>).  
        /// Defaults to <c>"week"</c>.
        /// </param>
        /// <returns>
        /// A list of <see cref="MovieFromAPI"/> objects with complete movie details.
        /// </returns>
        Task<List<MovieFromAPI>> GetTrendingMoviesWithDetails(string timeWindow);
    }
}
