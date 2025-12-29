using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Bookings;
using MovieSpot.Services.Payments;
using Stripe;
using Xunit;

namespace MovieSpot.Tests.Controllers.Payments
{
    public class PaymentControllerTest
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IBookingService> _bookingServiceMock;
        private readonly Mock<ILogger<PaymentController>> _loggerMock;
        private readonly PaymentController _controller;

        public PaymentControllerTest()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _bookingServiceMock = new Mock<IBookingService>();
            _loggerMock = new Mock<ILogger<PaymentController>>();

            _controller = new PaymentController(
                _paymentServiceMock.Object,
                _bookingServiceMock.Object,
                _loggerMock.Object);
        }

        // =====================================================
        // GET ALL PAYMENTS
        // =====================================================

        [Fact]
        public void GetAllPayments_ShouldReturnOk_WhenPaymentsExist()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment { Id = 1, AmountPaid = 20, PaymentStatus = "Paid" }
            };

            _paymentServiceMock
                .Setup(s => s.GetAllPayments())
                .Returns(payments);

            // Act
            var result = _controller.GetAllPayments();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<PaymentResponseDto>>(ok.Value);

            Assert.Single(value);
        }

        [Fact]
        public void GetAllPayments_ShouldReturnNotFound_WhenNoPaymentsExist()
        {
            // Arrange
            _paymentServiceMock
                .Setup(s => s.GetAllPayments())
                .Throws(new InvalidOperationException("No payments"));

            // Act
            var result = _controller.GetAllPayments();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);

            var json = System.Text.Json.JsonSerializer.Serialize(notFound.Value);
            Assert.Contains("No payments", json);
        }

        [Fact]
        public void GetAllPayments_ShouldReturnProblem_WhenUnexpectedError()
        {
            // Arrange
            _paymentServiceMock
                .Setup(s => s.GetAllPayments())
                .Throws(new Exception("Unexpected"));

            // Act
            var result = _controller.GetAllPayments();

            // Assert
            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, problem.StatusCode);

            var details = Assert.IsType<ProblemDetails>(problem.Value);
            Assert.Equal("Unexpected", details.Detail);
        }

        // =====================================================
        // ADD PAYMENT
        // =====================================================

        [Fact]
        public void AddPayment_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var booking = new Booking { Id = 1, TotalAmount = 50 };
            var request = new CreatePaymentRequestDto { BookingId = 1 };

            _bookingServiceMock
                .Setup(s => s.GetBookingById(1))
                .Returns(booking);

            _paymentServiceMock
                .Setup(s => s.ProcessStripePayment(booking, null))
                .Returns("secret-123");

            // Act
            var result = _controller.AddPayment(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<StripeIntentResponseDto>(ok.Value);

            Assert.Equal("secret-123", dto.ClientSecret);
        }

        [Fact]
        public void AddPayment_ShouldReturnBadRequest_WhenArgumentOutOfRangeException()
        {
            SetupAddPaymentThrow<ArgumentOutOfRangeException>();

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void AddPayment_ShouldReturnBadRequest_WhenArgumentNullException()
        {
            SetupAddPaymentThrow<ArgumentNullException>();

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void AddPayment_ShouldReturnNotFound_WhenKeyNotFoundException()
        {
            SetupAddPaymentThrow<KeyNotFoundException>();

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void AddPayment_ShouldReturnConflict_WhenInvalidOperationException()
        {
            SetupAddPaymentThrow<InvalidOperationException>();

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public void AddPayment_ShouldReturnInternalServerError_WhenDbUpdateException()
        {
            SetupAddPaymentThrow<DbUpdateException>();

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);

            Assert.IsType<ProblemDetails>(obj.Value);
        }

        [Fact]
        public void AddPayment_ShouldReturnBadGateway_WhenStripeException()
        {
            SetupAddPaymentThrow(() => new StripeException("Stripe fail", null));

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(502, obj.StatusCode);
        }

        [Fact]
        public void AddPayment_ShouldReturnInternalServerError_WhenGenericException()
        {
            SetupAddPaymentThrow(() => new Exception("Generic error"));

            var result = _controller.AddPayment(new CreatePaymentRequestDto { BookingId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);

            Assert.IsType<ProblemDetails>(obj.Value);
        }

        /*[Fact]
        public void AddPayment_ShouldReturnValidationProblem_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("BookingId", "Required");

            var result = _controller.AddPayment(new CreatePaymentRequestDto());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }*/

        // =====================================================
        // CHECK PAYMENT STATUS
        // =====================================================

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnOk_WhenSuccessful()
        {
            _paymentServiceMock
                .Setup(s => s.CheckPaymentStatus("sess_1"))
                .ReturnsAsync("paid");

            var result = await _controller.CheckPaymentStatus("sess_1");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<CheckPaymentStatusResponseDto>(ok.Value);

            Assert.Equal("paid", dto.Status);
        }

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnBadRequest_WhenArgumentException()
        {
            SetupCheckStatusThrow<ArgumentException>();

            var result = await _controller.CheckPaymentStatus("sess_1");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnNotFound_WhenKeyNotFoundException()
        {
            SetupCheckStatusThrow<KeyNotFoundException>();

            var result = await _controller.CheckPaymentStatus("sess_1");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnConflict_WhenInvalidOperationException()
        {
            SetupCheckStatusThrow<InvalidOperationException>();

            var result = await _controller.CheckPaymentStatus("sess_1");

            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnInternalServerError_WhenDbUpdateException()
        {
            SetupCheckStatusThrow<DbUpdateException>();

            var result = await _controller.CheckPaymentStatus("sess_1");

            var obj = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(500, obj.StatusCode);
            Assert.IsType<ProblemDetails>(obj.Value);
        }

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnBadGateway_WhenStripeException()
        {
            SetupCheckStatusThrow(() => new StripeException("Stripe fail", null));

            var result = await _controller.CheckPaymentStatus("sess_1");

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(502, obj.StatusCode);
        }

        [Fact]
        public async Task CheckPaymentStatus_ShouldReturnInternalServerError_WhenGenericException()
        {
            SetupCheckStatusThrow(() => new Exception("Generic error"));

            var result = await _controller.CheckPaymentStatus("sess_1");

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);

            Assert.IsType<ProblemDetails>(obj.Value);
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private void SetupAddPaymentThrow<T>() where T : Exception, new()
        {
            SetupAddPaymentThrow(() => new T());
        }

        private void SetupAddPaymentThrow(Func<Exception> exceptionFactory)
        {
            var booking = new Booking { Id = 1, TotalAmount = 100 };

            _bookingServiceMock
                .Setup(b => b.GetBookingById(1))
                .Returns(booking);

            _paymentServiceMock
                .Setup(p => p.ProcessStripePayment(booking, null))
                .Throws(exceptionFactory());
        }

        private void SetupCheckStatusThrow<T>() where T : Exception, new()
        {
            SetupCheckStatusThrow(() => new T());
        }

        private void SetupCheckStatusThrow(Func<Exception> exceptionFactory)
        {
            _paymentServiceMock
                .Setup(p => p.CheckPaymentStatus("sess_1"))
                .Throws(exceptionFactory());
        }
    }
}
