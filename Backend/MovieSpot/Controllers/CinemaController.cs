using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSpot.Models;
using MovieSpot.Services.Cinemas;
using MovieSpot.DTO_s;
using Microsoft.AspNetCore.Authorization;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Provides RESTful endpoints for managing cinemas,
    /// including retrieval, creation, update, and deletion.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CinemasController : ControllerBase
    {
        private readonly ICinemaService _cinemaService;
        private readonly ILogger<CinemasController> _logger;

        public CinemasController(ICinemaService cinemaService, ILogger<CinemasController> logger)
        {
            _cinemaService = cinemaService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all the cinemas registered in the system
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(IEnumerable<CinemaDTO.CinemaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<CinemaDTO.CinemaResponseDto>> GetCinemas()
        {
            try
            {
                var cinemas = _cinemaService.GetAllCinemas();

                var response = cinemas.Select(c => new CinemaDTO.CinemaResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Street = c.Street,
                    City = c.City,
                    State = c.State,
                    ZipCode = c.ZipCode,
                    Country = c.Country,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                });

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogInformation(ex, "No cinemas available to list.");
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific cinema by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema.</param>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(CinemaDTO.CinemaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CinemaDTO.CinemaResponseDto> GetCinema(int id)
        {
            try
            {
                var cinema = _cinemaService.GetCinemaById(id);

                var response = new CinemaDTO.CinemaResponseDto
                {
                    Id = cinema.Id,
                    Name = cinema.Name,
                    Street = cinema.Street,
                    City = cinema.City,
                    State = cinema.State,
                    ZipCode = cinema.ZipCode,
                    Country = cinema.Country,
                    Latitude = cinema.Latitude,
                    Longitude = cinema.Longitude,
                    CreatedAt = cinema.CreatedAt,
                    UpdatedAt = cinema.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Invalid cinema ID.");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogInformation(ex, "Cinema not found.");
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new cinema record in the system.
        /// </summary>
        /// <param name="dto">The cinema data used to create the record.</param>
        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(CinemaDTO.CinemaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<CinemaDTO.CinemaResponseDto> AddCinema([FromBody] CinemaDTO.CinemaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var cinema = new Cinema
                {
                    Name = dto.Name,
                    Street = dto.Street,
                    City = dto.City,
                    State = dto.State,
                    ZipCode = dto.ZipCode,
                    Country = dto.Country,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _cinemaService.AddCinema(cinema);

                var response = new CinemaDTO.CinemaResponseDto
                {
                    Id = cinema.Id,
                    Name = cinema.Name,
                    Street = cinema.Street,
                    City = cinema.City,
                    State = cinema.State,
                    ZipCode = cinema.ZipCode,
                    Country = cinema.Country,
                    Latitude = cinema.Latitude,
                    Longitude = cinema.Longitude,
                    CreatedAt = cinema.CreatedAt,
                    UpdatedAt = cinema.UpdatedAt
                };

                return CreatedAtAction(nameof(GetCinema), new { id = response.Id }, response);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Cinema object is null during creation.");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating cinema.");
                return Conflict(new { message = "Failed to create cinema record." });
            }
        }

        /// <summary>
        /// Updates an existing cinema with new data.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema to update.</param>
        /// <param name="dto">The updated cinema data.</param>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(CinemaDTO.CinemaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<CinemaDTO.CinemaResponseDto> UpdateCinema(int id, [FromBody] CinemaDTO.CinemaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (id != dto.Id)
                return BadRequest(new { message = "Path ID does not match cinema ID." });

            try
            {
                var existing = _cinemaService.GetCinemaById(id);

                existing.Name = dto.Name;
                existing.Street = dto.Street;
                existing.City = dto.City;
                existing.State = dto.State;
                existing.ZipCode = dto.ZipCode;
                existing.Country = dto.Country;
                existing.Latitude = dto.Latitude;
                existing.Longitude = dto.Longitude;
                existing.UpdatedAt = DateTime.UtcNow;

                _cinemaService.UpdateCinema(existing);

                var response = new CinemaDTO.CinemaResponseDto
                {
                    Id = existing.Id,
                    Name = existing.Name,
                    Street = existing.Street,
                    City = existing.City,
                    State = existing.State,
                    ZipCode = existing.ZipCode,
                    Country = existing.Country,
                    Latitude = existing.Latitude,
                    Longitude = existing.Longitude,
                    CreatedAt = existing.CreatedAt,
                    UpdatedAt = existing.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Cinema object is null during update.");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogInformation(ex, "Cinema not found for update.");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating cinema.");
                return Conflict(new { message = "Failed to update cinema record." });
            }
        }

        /// <summary>
        /// Deletes a cinema from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the cinema to remove.</param>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult DeleteCinema(int id)
        {
            try
            {
                _cinemaService.RemoveCinema(id);
                return NoContent();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Invalid cinema ID for deletion.");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogInformation(ex, "Cinema not found for deletion.");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting cinema.");
                return Conflict(new { message = "Failed to delete cinema record." });
            }
        }
    }
}
