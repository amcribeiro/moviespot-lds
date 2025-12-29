using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Seats;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Exposes endpoints for managing cinema seats.
    /// Allows listing, retrieving, creating, updating, and deleting seats.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SeatController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly ILogger<SeatController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="SeatController"/>.
        /// </summary>
        public SeatController(ISeatService seatService, ILogger<SeatController> logger)
        {
            _seatService = seatService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all seats in the system.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ActionName(nameof(GetAllSeats))]
        [ProducesResponseType(typeof(IEnumerable<SeatDTO.SeatResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SeatDTO.SeatResponseDto>>> GetAllSeats()
        {
            try
            {
                var seats = await _seatService.GetAllSeatsAsync();

                if (seats == null || !seats.Any())
                    return NotFound(new { message = "No seats found." });

                var response = seats.Select(s => new SeatDTO.SeatResponseDto
                {
                    Id = s.Id,
                    CinemaHallId = s.CinemaHallId,
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving seats.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Returns a seat by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Staff")]
        [ActionName(nameof(GetSeatById))]
        [ProducesResponseType(typeof(SeatDTO.SeatResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeatDTO.SeatResponseDto>> GetSeatById(int id)
        {
            try
            {
                var seat = await _seatService.GetSeatByIdAsync(id);
                if (seat == null)
                    return NotFound(new { message = $"Seat #{id} not found." });

                var response = new SeatDTO.SeatResponseDto
                {
                    Id = seat.Id,
                    CinemaHallId = seat.CinemaHallId,
                    SeatNumber = seat.SeatNumber,
                    SeatType = seat.SeatType,
                    CreatedAt = seat.CreatedAt,
                    UpdatedAt = seat.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving seat by ID.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Returns all seats from a specific cinema hall.
        /// </summary>
        [HttpGet("hall/{cinemaHallId:int}")]
        [Authorize(Roles = "User,Staff")]
        [ActionName(nameof(GetSeatsByCinemaHall))]
        [ProducesResponseType(typeof(IEnumerable<SeatDTO.SeatResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SeatDTO.SeatResponseDto>>> GetSeatsByCinemaHall(int cinemaHallId)
        {
            try
            {
                var seats = await _seatService.GetSeatsByCinemaHallIdAsync(cinemaHallId);

                if (seats == null || !seats.Any())
                    return NotFound(new { message = $"No seats found for CinemaHall #{cinemaHallId}." });

                var response = seats.Select(s => new SeatDTO.SeatResponseDto
                {
                    Id = s.Id,
                    CinemaHallId = s.CinemaHallId,
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving seats by CinemaHallId.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Creates a new seat.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ActionName(nameof(AddSeat))]
        [ProducesResponseType(typeof(SeatDTO.SeatResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<SeatDTO.SeatResponseDto>> AddSeat([FromBody] SeatDTO.SeatCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var seat = new Seat
                {
                    CinemaHallId = dto.CinemaHallId,
                    SeatNumber = dto.SeatNumber,
                    SeatType = dto.SeatType
                };

                var created = await _seatService.AddSeatAsync(seat);

                var response = new SeatDTO.SeatResponseDto
                {
                    Id = created.Id,
                    CinemaHallId = created.CinemaHallId,
                    SeatNumber = created.SeatNumber,
                    SeatType = created.SeatType,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt
                };

                return CreatedAtAction(nameof(GetSeatById), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while adding seat.");
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid seat data.");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating seat.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Database error",
                    detail: "Error while creating seat.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating seat.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing seat.
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Staff")]
        [ActionName(nameof(UpdateSeat))]
        [ProducesResponseType(typeof(SeatDTO.SeatResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<SeatDTO.SeatResponseDto>> UpdateSeat([FromBody] SeatDTO.SeatUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var seat = new Seat
                {
                    Id = dto.Id,
                    CinemaHallId = dto.CinemaHallId,
                    SeatNumber = dto.SeatNumber,
                    SeatType = dto.SeatType
                };

                var updated = await _seatService.UpdateSeatAsync(seat);

                var response = new SeatDTO.SeatResponseDto
                {
                    Id = updated.Id,
                    CinemaHallId = updated.CinemaHallId,
                    SeatNumber = updated.SeatNumber,
                    SeatType = updated.SeatType,
                    CreatedAt = updated.CreatedAt,
                    UpdatedAt = updated.UpdatedAt
                };

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Seat not found.");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while updating seat.");
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating seat.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Database error",
                    detail: "Error while updating seat.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating seat.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Deletes a seat by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Staff")]
        [ActionName(nameof(DeleteSeat))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            try
            {
                var deleted = await _seatService.RemoveSeatAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Seat #{id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting seat.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Returns a seat with its calculated price for a given session.
        /// </summary>
        [HttpGet("{seatId:int}/price/{sessionId:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(SeatDTO.SeatResponsePriceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeatDTO.SeatResponsePriceDto>> GetSeatPrice(int seatId, int sessionId)
        {
            try
            {
                var seatPrice = await _seatService.GetSeatPriceAsync(seatId, sessionId);

                if (seatPrice == null)
                    return NotFound(new { message = $"Seat #{seatId} or Session #{sessionId} not found." });

                return Ok(seatPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving seat price.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }
    }
}
