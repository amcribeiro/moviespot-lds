using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSpot.Models;
using MovieSpot.Services.Bookings;
using static MovieSpot.DTO_s.BookingDTO;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for managing bookings.
    /// Returns 200 on success, 400 for validation or invalid operation errors,
    /// and 404 when a booking is not found.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        /// <summary>
        /// Initializes a new instance of <see cref="BookingController"/>.
        /// </summary>
        /// <param name="bookingService">The booking service.</param>
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Retrieves all bookings in the system.
        /// </summary>
        /// <returns>A list of all bookings.</returns>
        /// <response code="200">Bookings successfully retrieved.</response>
        /// <response code="404">No bookings found.</response>
        [HttpGet]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetAll()
        {
            try
            {
                var bookings = _bookingService.GetAllBookings();
                var response = bookings.Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    SessionId = b.SessionId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
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
        /// Retrieves all bookings made by a specific user.
        /// </summary>
        /// <param name="userId">The user's unique ID.</param>
        /// <returns>A list of bookings belonging to the user.</returns>
        /// <response code="200">Bookings successfully retrieved.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="404">No bookings found for the user.</response>
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Staff,User")]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetByUser(int userId)
        {
            try
            {
                var bookings = _bookingService.GetAllBookingsByUserId(userId);
                var response = bookings.Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    SessionId = b.SessionId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                });

                return Ok(response);
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
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a booking by its unique ID.
        /// </summary>
        /// <param name="id">The booking ID.</param>
        /// <returns>The booking matching the provided ID.</returns>
        /// <response code="200">Booking successfully retrieved.</response>
        /// <response code="400">Invalid booking ID.</response>
        /// <response code="404">Booking not found.</response>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Staff,User")]
        [ProducesResponseType(typeof(BookingResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            try
            {
                var booking = _bookingService.GetBookingById(id);
                var response = new BookingResponseDto
                {
                    Id = booking.Id,
                    UserId = booking.UserId,
                    SessionId = booking.SessionId,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,
                    TotalAmount = booking.TotalAmount,
                    CreatedAt = booking.CreatedAt,
                    UpdatedAt = booking.UpdatedAt
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
        /// Creates a new booking.
        /// </summary>
        /// <param name="newBookingDto">The data required to create the booking.</param>
        /// <returns>The newly created booking.</returns>
        /// <response code="200">Booking successfully created.</response>
        /// <response code="400">Invalid data or database error.</response>
        [HttpPost]
        [Authorize(Roles = "Staff,User")]
        [ProducesResponseType(typeof(BookingResponseDto), 200)]
        [ProducesResponseType(400)]
        public IActionResult Create([FromBody] BookingCreateDto newBookingDto)
        {
            if (newBookingDto == null)
                return BadRequest("Booking data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newBooking = new Booking
                {
                    UserId = newBookingDto.UserId,
                    SessionId = newBookingDto.SessionId,
                    BookingDate = DateTime.UtcNow,
                    Status = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = _bookingService.CreateBookingWithSeats(newBooking, newBookingDto.SeatIds);

                var response = new BookingResponseDto
                {
                    Id = created.Id,
                    UserId = created.UserId,
                    SessionId = created.SessionId,
                    BookingDate = created.BookingDate,
                    Status = created.Status,
                    TotalAmount = created.TotalAmount,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing booking.
        /// </summary>
        /// <param name="id">The booking ID to update.</param>
        /// <param name="updatedBookingDto">The updated booking data.</param>
        /// <returns>The updated booking.</returns>
        /// <response code="200">Booking successfully updated.</response>
        /// <response code="400">Invalid data or incorrect ID.</response>
        /// <response code="404">Booking not found.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Staff,User")]
        [ProducesResponseType(typeof(BookingResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Update(int id, [FromBody] BookingUpdateDto updatedBookingDto)
        {
            if (updatedBookingDto == null)
                return BadRequest("Booking data cannot be null.");

            if (id != updatedBookingDto.Id)
                return BadRequest("The provided ID does not match the booking ID.");

            try
            {
                var updatedBooking = new Booking
                {
                    Id = updatedBookingDto.Id,
                    UserId = updatedBookingDto.UserId,
                    SessionId = updatedBookingDto.SessionId,
                    Status = updatedBookingDto.Status,
                    TotalAmount = updatedBookingDto.TotalAmount,
                    UpdatedAt = DateTime.UtcNow
                };

                var updated = _bookingService.UpdateBooking(id, updatedBooking);

                var response = new BookingResponseDto
                {
                    Id = updated.Id,
                    UserId = updated.UserId,
                    SessionId = updated.SessionId,
                    BookingDate = updated.BookingDate,
                    Status = updated.Status,
                    TotalAmount = updated.TotalAmount,
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

        [HttpGet("stats/peak-hours")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<PeakHourDto>> GetPeakHours()
        {
            try
            {
                var stats = _bookingService.GetPeakBookingHours();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
