using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Sessions;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for managing cinema sessions.
    /// Provides CRUD operations for session management.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        /// <summary>
        /// Retrieves a session by its ID.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        /// <returns>The matching session.</returns>
        /// <response code="200">Session found successfully.</response>
        /// <response code="400">Invalid ID.</response>
        /// <response code="404">Session not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SessionDTO.SessionResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            try
            {
                var session = _sessionService.GetSessionById(id);

                var response = new SessionDTO.SessionResponseDto
                {
                    Id = session.Id,
                    MovieId = session.MovieId,
                    MovieTitle = session.Movie?.Title ?? "Unknown",
                    CinemaHallId = session.CinemaHallId,
                    CinemaHallName = session.CinemaHall?.Name ?? "Unknown",
                    CreatedBy = session.CreatedBy,
                    CreatedByName = session.CreatedByUser?.Name ?? "Unknown",
                    StartDate = session.StartDate,
                    EndDate = session.EndDate,
                    Price = session.Price,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt
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
        }

        /// <summary>
        /// Retrieves all cinema sessions.
        /// </summary>
        /// <returns>List of sessions.</returns>
        /// <response code="200">Sessions retrieved successfully.</response>
        /// <response code="404">No sessions found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SessionDTO.SessionResponseDto>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetAll()
        {
            try
            {
                var sessions = _sessionService.GetAllSessions();
                var response = sessions.Select(s => new SessionDTO.SessionResponseDto
                {
                    Id = s.Id,
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie?.Title ?? "Unknown",
                    CinemaHallId = s.CinemaHallId,
                    CinemaHallName = s.CinemaHall?.Name ?? "Unknown",
                    CreatedBy = s.CreatedBy,
                    CreatedByName = s.CreatedByUser?.Name ?? "Unknown",
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Price = s.Price,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                });

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new cinema session.
        /// </summary>
        /// <param name="newSessionDto">Session data for creation.</param>
        /// <returns>The created session.</returns>
        /// <response code="200">Session created successfully.</response>
        /// <response code="400">Invalid input or conflicting schedule.</response>
        [HttpPost]
        [ProducesResponseType(typeof(SessionDTO.SessionResponseDto), 200)]
        [ProducesResponseType(400)]
        public IActionResult Create([FromBody] SessionDTO.SessionCreateDto newSessionDto)
        {
            if (newSessionDto is null)
                return BadRequest("Session cannot be null.");

            try
            {
                var session = new Session
                {
                    MovieId = newSessionDto.MovieId,
                    CinemaHallId = newSessionDto.CinemaHallId,
                    CreatedBy = newSessionDto.CreatedBy,
                    StartDate = newSessionDto.StartDate,
                    EndDate = newSessionDto.EndDate,
                    Price = newSessionDto.Price
                };

                var created = _sessionService.CreateSession(session);

                var response = new SessionDTO.SessionResponseDto
                {
                    Id = created.Id,
                    MovieId = created.MovieId,
                    CinemaHallId = created.CinemaHallId,
                    CreatedBy = created.CreatedBy,
                    StartDate = created.StartDate,
                    EndDate = created.EndDate,
                    Price = created.Price,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
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
        }

        /// <summary>
        /// Updates an existing cinema session.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        /// <param name="updatedDto">Updated session data.</param>
        /// <returns>The updated session.</returns>
        /// <response code="200">Session updated successfully.</response>
        /// <response code="400">Invalid input or schedule conflict.</response>
        /// <response code="404">Session not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(SessionDTO.SessionResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Update(int id, [FromBody] SessionDTO.SessionUpdateDto updatedDto)
        {
            if (updatedDto is null)
                return BadRequest("Session cannot be null.");

            try
            {
                var session = new Session
                {
                    MovieId = updatedDto.MovieId,
                    CinemaHallId = updatedDto.CinemaHallId,
                    StartDate = updatedDto.StartDate,
                    EndDate = updatedDto.EndDate,
                    Price = updatedDto.Price
                };

                var updated = _sessionService.UpdateSession(id, session);

                var response = new SessionDTO.SessionResponseDto
                {
                    Id = updated.Id,
                    MovieId = updated.MovieId,
                    CinemaHallId = updated.CinemaHallId,
                    CreatedBy = updated.CreatedBy,
                    StartDate = updated.StartDate,
                    EndDate = updated.EndDate,
                    Price = updated.Price,
                    CreatedAt = updated.CreatedAt,
                    UpdatedAt = updated.UpdatedAt
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
            catch (InvalidOperationException ex)
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
        }

        /// <summary>
        /// Deletes a session by ID.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        /// <returns>The deleted session.</returns>
        /// <response code="200">Session deleted successfully.</response>
        /// <response code="400">Invalid ID.</response>
        /// <response code="404">Session not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(SessionDTO.SessionResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            try
            {
                var deleted = _sessionService.DeleteSession(id);
                var response = new SessionDTO.SessionResponseDto
                {
                    Id = deleted.Id,
                    MovieId = deleted.MovieId,
                    CinemaHallId = deleted.CinemaHallId,
                    CreatedBy = deleted.CreatedBy,
                    StartDate = deleted.StartDate,
                    EndDate = deleted.EndDate,
                    Price = deleted.Price,
                    CreatedAt = deleted.CreatedAt,
                    UpdatedAt = deleted.UpdatedAt
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
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all available time slots for a given cinema hall and date.
        /// </summary>
        /// <param name="cinemaHallId">The unique identifier of the cinema hall.</param>
        /// <param name="date">The date to check availability.</param>
        /// <param name="runtimeMinutes">The duration of the movie in minutes.</param>
        /// <returns>List of available time slots.</returns>
        /// <response code="200">Returns the list of available time slots.</response>
        /// <response code="400">Invalid parameters.</response>
        /// <response code="404">No available times found.</response>
        [HttpGet("available-times")]
        [ProducesResponseType(typeof(IEnumerable<TimeSpan>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetAvailableTimes([FromQuery] int cinemaHallId, [FromQuery] DateTime date, [FromQuery] int runtimeMinutes)
        {
            try
            {
                var availableTimes = _sessionService.GetAvailableTimes(cinemaHallId, date, runtimeMinutes);

                return Ok(availableTimes);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error while retrieving available times: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all available seats for a given session, excluding those already booked.
        /// </summary>
        /// <param name="sessionId">Session identifier.</param>
        /// <returns>List of available seats.</returns>
        /// <response code="200">Returns the list of available seats.</response>
        /// <response code="400">Invalid session ID.</response>
        /// <response code="404">Session not found or no available seats.</response>
        [HttpGet("{sessionId:int}/available-seats")]
        [ProducesResponseType(typeof(IEnumerable<SessionDTO.AvailableSeatDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetAvailableSeats(int sessionId)
        {
            try
            {
                var seats = _sessionService.GetAvailableSeats(sessionId);

                var response = seats.Select(s => new SessionDTO.AvailableSeatDto
                {
                    Id = s.Id,
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType
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
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error while retrieving available seats: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém a taxa de ocupação de uma sessão específica.
        /// </summary>
        /// <param name="id">ID da sessão.</param>
        [HttpGet("{id:int}/occupancy")]
        [Authorize(Roles = "Staff")]
        public ActionResult<SessionOccupancyDto> GetOccupancy(int id)
        {
            try
            {
                var occupancy = _sessionService.GetSessionOccupancy(id);
                return Ok(occupancy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, ...);
                return BadRequest($"Erro inesperado: {ex.Message}");
            }
        }
    }
}
