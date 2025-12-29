using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieSpot.Models;
using MovieSpot.Services.Genres;
using static MovieSpot.DTO_s.GenreDTO;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Exposes genre-related HTTP endpoints.
    /// Base route: <c>/genre</c>.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        private readonly ILogger<GenreController> _logger;

        public GenreController(IGenreService genreService, ILogger<GenreController> logger)
        {
            _genreService = genreService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all genres stored in the local database.
        /// </summary>
        /// <returns>
        /// 200 OK with the list of <see cref="Genre"/>; 
        /// 404 Not Found when there are no genres; 
        /// 500 on unexpected error.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(List<GenreResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<GenreResponseDto>> GetGenres()
        {
            try
            {
                var genres = _genreService.GetAllGenresFromDb();

                if (genres == null || genres.Count == 0)
                    return NotFound("No genres found in the database.");

                var genreDtos = genres.Select(g => new GenreResponseDto
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToList();

                return Ok(genreDtos);
            }
            catch (InvalidOperationException)
            {
                return NotFound("No genres found in the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving genres from DB.");
                return StatusCode(500, "An error occurred while retrieving genres.");
            }
        }

        /// <summary>
        /// Returns a single genre by its identifier.
        /// </summary>
        /// <param name="id">Genre identifier (&gt; 0).</param>
        /// <returns>
        /// 200 OK with the <see cref="Genre"/>; 
        /// 400 Bad Request if id invalid; 
        /// 404 Not Found if missing; 
        /// 500 on unexpected error.
        /// </returns>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(GenreResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<GenreResponseDto> GetGenre(int id)
        {
            if (id <= 0)
                return BadRequest("Genre ID must be greater than zero.");

            try
            {
                var genre = _genreService.GetGenreById(id);

                var genreDto = new GenreResponseDto
                {
                    Id = genre.Id,
                    Name = genre.Name
                };

                return Ok(genreDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Genre with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving genre {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the genre.");
            }
        }

        /// <summary>
        /// Synchronizes genres with the external API (TMDB) and upserts into the local database.
        /// </summary>
        /// <remarks>
        /// Useful to seed/refresh genres. Does not return payload; only status.
        /// </remarks>
        /// <returns>204 No Content on success; 500 on error.</returns>
        [HttpPost("sync")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SyncGenres()
        {
            try
            {
                await _genreService.SyncGenresAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while synchronizing genres with TMDB.");
                return StatusCode(500, "An error occurred while synchronizing genres.");
            }
        }

        /// <summary>
        /// Retorna estatísticas sobre os géneros (nº de filmes e sessões).
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<GenreStatDto>> GetGenreStats()
        {
            try
            {
                var stats = _genreService.GetGenreStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de géneros.");
                return StatusCode(500, "Erro interno ao processar estatísticas.");
            }
        }
    }
}
