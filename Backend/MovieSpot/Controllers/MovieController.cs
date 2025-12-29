using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Movies;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Exposes movie-related HTTP endpoints.
    /// Requires authentication via JWT (see <see cref="AuthorizeAttribute"/>).
    /// Base route: <c>/movie</c>.
    /// </summary>
    /// [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<MovieController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="MovieController"/>.
        /// </summary>
        /// <param name="movieService">Movie business logic abstraction.</param>
        /// <param name="logger">Typed logger for this controller.</param>
        public MovieController(IMovieService movieService, ILogger<MovieController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all local movies including their genres.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="MovieDto"/> entities and their genres.</description></item>
        /// <item><description><c>404 Not Found</c> when no movies are found.</description></item>
        /// <item><description><c>500 Internal Server Error</c> when an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(List<MovieDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<MovieDto>> GetAllMovies()
        {
            try
            {
                var movies = _movieService.GetMovies();

                if (movies == null || movies.Count == 0)
                    return NotFound("No movies found in the local database.");

                var movieDtos = movies.Select(m => new MovieDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Duration = m.Duration,
                    Language = m.Language,
                    ReleaseDate = m.ReleaseDate,
                    Country = m.Country,
                    PosterPath = $"https://image.tmdb.org/t/p/w500{m.PosterPath}",
                    Genres = m.MovieGenres
                        .Select(mg => mg.Genre.Name)
                        .Distinct()
                        .ToList()
                }).ToList();

                return Ok(movieDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving all movies.");
                return StatusCode(500, "An error occurred while retrieving the movies.");
            }
        }

        /// <summary>
        /// Retrieves a movie by its identifier from the local database.
        /// </summary>
        /// <param name="id">The movie identifier. Must be greater than zero.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the <see cref="MovieDto"/> entity.</description></item>
        /// <item><description><c>400 Bad Request</c> when <paramref name="id"/> is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> when the movie does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected errors.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<MovieDto> GetMovieById(int id)
        {
            if (id <= 0)
                return BadRequest("Movie ID must be greater than zero.");

            try
            {
                var movie = _movieService.GetMovie(id);
                if (movie == null)
                    return NotFound($"Movie with ID {id} was not found.");

                var dto = new MovieDto
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Description = movie.Description,
                    Duration = movie.Duration,
                    Language = movie.Language,
                    ReleaseDate = movie.ReleaseDate,
                    Country = movie.Country,
                    PosterPath = $"https://image.tmdb.org/t/p/w500{movie.PosterPath}",
                    Genres = movie.MovieGenres
                        .Select(mg => mg.Genre.Name)
                        .Distinct()
                        .ToList()
                };

                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Movie with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving movie {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the movie.");
            }
        }

        /// <summary>
        /// Adds a new movie to the local database.
        /// </summary>
        /// <param name="movie">The movie payload to insert. Cannot be null.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>201 Created</c> with Location header pointing to <c>/movie/{id}</c>.</description></item>
        /// <item><description><c>400 Bad Request</c> when payload is null.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected errors.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("add")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult AddMovie([FromBody] Movie movie)
        {
            if (movie is null)
                return BadRequest("Movie data is required.");

            try
            {
                _movieService.AddMovie(movie);
                return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding a movie.");
                return StatusCode(500, "An error occurred while adding the movie.");
            }
        }

        /// <summary>
        /// Removes a movie by its identifier from the local database.
        /// </summary>
        /// <param name="id">The movie identifier. Must be greater than zero.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 No Content</c> on successful deletion.</description></item>
        /// <item><description><c>400 Bad Request</c> when <paramref name="id"/> is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> when the movie does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected errors.</description></item>
        /// </list>
        /// </returns>
        [HttpDelete("remove/{id:int}")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult RemoveMovie(int id)
        {
            if (id <= 0)
                return BadRequest("Movie ID must be greater than zero.");

            try
            {
                _movieService.RemoveMovie(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Movie with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while removing movie {Id}", id);
                return StatusCode(500, "An error occurred while removing the movie.");
            }
        }

        /// <summary>
        /// Retorna a contagem de sessões por filme.
        /// </summary>
        [HttpGet("stats/sessions-count")]
        [Authorize(Roles = "Staff")] // Apenas staff deve ver relatórios
        public ActionResult<List<MovieSessionCountDto>> GetSessionCounts()
        {
            try
            {
                var stats = _movieService.GetSessionCountsPerMovie();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter contagem de sessões.");
                return StatusCode(500, "Erro interno ao processar estatísticas.");
            }
        }

        /// <summary>
        /// Retorna a receita total gerada por cada filme.
        /// </summary>
        [HttpGet("stats/revenue")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<MovieRevenueDto>> GetRevenue()
        {
            try
            {
                var stats = _movieService.GetRevenuePerMovie();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receita por filme.");
                return StatusCode(500, "Erro interno ao processar estatísticas.");
            }
        }

        [HttpGet("stats/popular")]
        [Authorize(Roles = "Staff,User")] // Users podem querer ver o top
        public ActionResult<List<PopularMovieDto>> GetPopularMovies()
        {
            try
            {
                var result = _movieService.GetMostPopularMovies();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}