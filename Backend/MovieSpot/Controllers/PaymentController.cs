using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Bookings;
using MovieSpot.Services.Payments;
using Stripe;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Exposes payment endpoints: list all payments, create a Stripe checkout session, and check payment status.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    // [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="PaymentController"/>.
        /// </summary>
        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all payments from the system.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "User,Staff")]
        [ActionName(nameof(GetAllPayments))]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<PaymentResponseDto>> GetAllPayments()
        {
            try
            {
                var payments = _paymentService.GetAllPayments();

                var response = payments.Select(p => new PaymentResponseDto
                {
                    Id = p.Id,
                    BookingId = p.BookingId,
                    VoucherId = p.VoucherId,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    AmountPaid = p.AmountPaid,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogInformation(ex, "No payments found.");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving payments.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Creates a Stripe Checkout Session for the specified booking (optional voucher).
        /// </summary>
        /// <param name="request">Booking identifier and optional voucher identifier.</param>
        [HttpPost("checkout")]
        [Authorize(Roles = "User,Staff")]
        [ActionName(nameof(AddPayment))]
        [ProducesResponseType(typeof(StripeIntentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<StripeIntentResponseDto> AddPayment([FromBody] CreatePaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var booking = _bookingService.GetBookingById(request.BookingId);
                var clientSecret = _paymentService.ProcessStripePayment(booking, request.VoucherId);

                return Ok(new StripeIntentResponseDto { ClientSecret = clientSecret });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Invalid argument value.");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking not found.");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid booking.");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation.");
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating payment.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Database error",
                    detail: "Error while creating payment.");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error while creating checkout session.");
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating checkout session.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        /// <summary>
        /// Checks Stripe PaymentIntent status and updates the last pending payment accordingly.
        /// </summary>
        /// <param name="paymentIntentId">Stripe PaymentIntent identifier.</param>
        [HttpGet("check-payment-status")]
        [ActionName(nameof(CheckPaymentStatus))]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(CheckPaymentStatusResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CheckPaymentStatusResponseDto>> CheckPaymentStatus(
            [FromQuery] string paymentIntentId
        )
        {
            try
            {
                var status = await _paymentService.CheckPaymentStatus(paymentIntentId);

                var response = new CheckPaymentStatusResponseDto
                {
                    Status = status
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid PaymentIntent id.");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "No matching pending payment found.");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Stripe PaymentIntent not found or invalid.");
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating payment status.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Database error",
                    detail: "Error while updating payment status");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error while checking payment status.");
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking payment status.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: ex.Message);
            }
        }

        [HttpGet("stats/methods")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<PaymentMethodStatDto>> GetPaymentStats()
        {
            try
            {
                var stats = _paymentService.GetPaymentMethodStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}