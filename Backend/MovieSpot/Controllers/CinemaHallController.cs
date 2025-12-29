using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSpot.Models;
using MovieSpot.Services.CinemaHalls;
using static MovieSpot.DTO_s.CinemaHallDTO;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for managing cinema halls (CinemaHalls).
    /// Returns 200 on success, 400 for validation errors or invalid operations,
    /// and 404 when the hall is not found.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CinemaHallController : ControllerBase
    {
        private readonly ICinemaHallService _cinemaHallService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CinemaHallController"/> class.
        /// </summary>
        /// <param name="cinemaHallService">Service responsible for cinema hall operations.</param>
        public CinemaHallController(ICinemaHallService cinemaHallService)
        {
            _cinemaHallService = cinemaHallService;
        }

        /// <summary>
        /// Retrieves all cinema halls in the system.
        /// </summary>
        /// <returns>A list of cinema halls.</returns>
        /// <response code="200">Cinema halls successfully found.</response>
        /// <response code="404">No cinema halls found.</response>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(IEnumerable<CinemaHallReadDto>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetAll()
        {
            try
            {
                var halls = _cinemaHallService.GetAllCinemaHalls();

                var response = halls.Select(h => new CinemaHallReadDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    CinemaId = h.CinemaId
                });

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a cinema hall by its ID.
        /// </summary>
        /// <param name="id">Identifier of the cinema hall.</param>
        /// <returns>The cinema hall that matches the given ID.</returns>
        /// <response code="200">Cinema hall found.</response>
        /// <response code="400">Invalid ID.</response>
        /// <response code="404">Cinema hall not found.</response>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(CinemaHallDetailsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            try
            {
                var hall = _cinemaHallService.GetCinemaHallById(id);

                var response = new CinemaHallDetailsDto
                {
                    Id = hall.Id,
                    Name = hall.Name,
                    CinemaId = hall.CinemaId,
                    CinemaName = hall.Cinema?.Name,
                    CreatedAt = hall.CreatedAt,
                    UpdatedAt = hall.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all cinema halls associated with a specific cinema.
        /// </summary>
        /// <param name="cinemaId">Cinema ID.</param>
        /// <returns>List of halls that belong to the given cinema.</returns>
        /// <response code="200">Cinema halls found.</response>
        /// <response code="400">Invalid cinema ID.</response>
        /// <response code="404">No cinema halls found for the given cinema.</response>
        [HttpGet("cinema/{cinemaId:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(IEnumerable<CinemaHallReadDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetByCinemaId(int cinemaId)
        {
            try
            {
                var halls = _cinemaHallService.GetCinemaHallsByCinemaId(cinemaId);

                var response = halls.Select(h => new CinemaHallReadDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    CinemaId = h.CinemaId
                });

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new cinema hall.
        /// </summary>
        /// <param name="newCinemaHallDto">Cinema hall data to create.</param>
        /// <returns>The created cinema hall.</returns>
        /// <response code="200">Cinema hall successfully created.</response>
        /// <response code="400">Invalid data or database error.</response>
        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(CinemaHallReadDto), 200)]
        [ProducesResponseType(400)]
        public IActionResult Create([FromBody] CinemaHallCreateDto newCinemaHallDto)
        {
            if (newCinemaHallDto == null)
                return BadRequest("Cinema hall data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newHall = new CinemaHall
                {
                    Name = newCinemaHallDto.Name,
                    CinemaId = newCinemaHallDto.CinemaId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = _cinemaHallService.AddCinemaHall(newHall);

                var response = new CinemaHallReadDto
                {
                    Id = created.Id,
                    Name = created.Name,
                    CinemaId = created.CinemaId
                };

                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing cinema hall.
        /// </summary>
        /// <param name="id">ID of the cinema hall to update.</param>
        /// <param name="updatedCinemaHallDto">New cinema hall data.</param>
        /// <returns>The updated cinema hall.</returns>
        /// <response code="200">Cinema hall successfully updated.</response>
        /// <response code="400">Invalid data or mismatched ID.</response>
        /// <response code="404">Cinema hall not found.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(CinemaHallReadDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Update(int id, [FromBody] CinemaHallUpdateDto updatedCinemaHallDto)
        {
            if (updatedCinemaHallDto == null)
                return BadRequest("Cinema hall data cannot be null.");

            if (id != updatedCinemaHallDto.Id)
                return BadRequest("The provided ID does not match the cinema hall ID.");

            try
            {
                var updatedHall = new CinemaHall
                {
                    Id = updatedCinemaHallDto.Id,
                    Name = updatedCinemaHallDto.Name,
                    CinemaId = updatedCinemaHallDto.CinemaId,
                    UpdatedAt = DateTime.UtcNow
                };

                var updated = _cinemaHallService.UpdateCinemaHall(id, updatedHall);

                var response = new CinemaHallReadDto
                {
                    Id = updated.Id,
                    Name = updated.Name,
                    CinemaId = updated.CinemaId
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a cinema hall by its ID.
        /// </summary>
        /// <param name="id">ID of the cinema hall to delete.</param>
        /// <response code="200">Cinema hall successfully deleted.</response>
        /// <response code="400">Invalid ID.</response>
        /// <response code="404">Cinema hall not found.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            try
            {
                _cinemaHallService.RemoveCinemaHall(id);
                return Ok($"Cinema hall with ID {id} was successfully deleted.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }
    }
}
