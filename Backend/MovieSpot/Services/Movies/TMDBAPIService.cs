using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MovieSpot.Models;
using MovieSpot.Services.Movies;

namespace MovieSpot.Services.Tmdb
{
    /// <summary>
    /// TMDB client that implements <see cref="ITMDBAPIService"/>.
    /// Responsible for consuming TMDB REST API endpoints and mapping the responses
    /// into the internal <see cref="MovieFromAPI"/> data model used by <see cref="MovieService"/>.
    /// </summary>
    /// <remarks>
    /// This implementation uses a typed <see cref="HttpClient"/> configured via dependency injection.
    /// Expected base URL: <c>https://api.themoviedb.org/3/</c><br/>
    /// Expected header: <c>Authorization: Bearer &lt;TMDB_V4_TOKEN&gt;</c>.
    /// </remarks>
    public sealed class TmdbApiService : ITMDBAPIService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of <see cref="TmdbApiService"/>.
        /// </summary>
        /// <param name="http">The injected HTTP client configured for TMDB API access.</param>
        public TmdbApiService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Retrieves a list of trending movies from TMDB.
        /// </summary>
        /// <param name="timeWindow">
        /// The time window for trending movies.
        /// Valid values are <c>"day"</c> or <c>"week"</c>.
        /// Defaults to <c>"week"</c>.
        /// </param>
        /// <returns>
        /// A list of <see cref="MovieFromAPI"/> objects containing trending movie data.
        /// Returns an empty list if TMDB returns no results or a null payload.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request to TMDB fails.
        /// </exception>
        public async Task<List<int>> GetTrendingMovies(string timeWindow = "week")
        {
            if (string.IsNullOrWhiteSpace(timeWindow))
                timeWindow = "week";

            var resp = await _http.GetFromJsonAsync<TmdbTrendingResponse>($"trending/movie/{timeWindow}");

            if (resp?.Results == null)
                return new();

            return resp.Results.Select(r => r.Id).ToList();
        }

        /// <summary>
        /// Retrieves a detailed movie record from TMDB by its unique identifier.
        /// </summary>
        /// <param name="id">The TMDB movie identifier.</param>
        /// <returns>
        /// A <see cref="MovieFromAPI"/> object containing full movie details,
        /// or <c>null</c> if the movie is not found.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request to TMDB fails.
        /// </exception>
        public async Task<MovieFromAPI?> GetMovieFromAPI(int id)
        {
            var movie = await _http.GetFromJsonAsync<MovieFromAPI>($"movie/{id}");

            if (movie == null)
                return null;

            movie.Title = movie.Title ?? string.Empty;
            movie.Overview = movie.Overview ?? string.Empty;
            movie.OriginalLanguage = movie.OriginalLanguage ?? string.Empty;
            movie.ReleaseDate = movie.ReleaseDate ?? string.Empty;
            movie.PosterPath = movie.PosterPath ?? string.Empty;
            movie.OriginCountry ??= new List<string>();
            movie.Genres ??= new List<Genre>();

            return movie;
        }

        /// <summary>
        /// Gets a list of trending movies from TMDB and fetches their full details.
        /// </summary>
        /// <param name="timeWindow">
        /// The time range for trending movies (<c>"day"</c> or <c>"week"</c>).  
        /// Defaults to <c>"week"</c>.
        /// </param>
        /// <returns>
        /// A list of <see cref="MovieFromAPI"/> objects with complete movie details.
        /// Returns an empty list if no movies are found.
        /// </returns>
        public async Task<List<MovieFromAPI>> GetTrendingMoviesWithDetails(string timeWindow)
        {
            if (string.IsNullOrWhiteSpace(timeWindow))
                timeWindow = "week";

            var ids = await GetTrendingMovies(timeWindow);
            var movies = new List<MovieFromAPI>();

            foreach (var id in ids)
            {
                var movie = await GetMovieFromAPI(id);
                if (movie != null)
                    movies.Add(movie);
            }

            return movies;
        }

        /// <summary>
        /// DTO representing a TMDB trending movies API response.
        /// </summary>
        private sealed class TmdbTrendingResponse
        {
            /// <summary>
            /// The list of trending movie results returned by TMDB.
            /// </summary>
            [JsonPropertyName("results")]
            public List<MovieFromAPI>? Results { get; set; }
        }
    }
}
