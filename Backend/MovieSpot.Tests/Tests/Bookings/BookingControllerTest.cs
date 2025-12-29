using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Bookings;
using Xunit;
using static MovieSpot.DTO_s.BookingDTO;

namespace MovieSpot.Tests.Controllers.Bookings
{
    public class BookingControllerTest
    {
        private readonly Mock<IBookingService> _mockService;
        private readonly BookingController _controller;

        public BookingControllerTest()
        {
            _mockService = new Mock<IBookingService>();
            _controller = new BookingController(_mockService.Object);
        }

        private Booking CreateValidBooking() => new Booking
        {
            Id = 1,
            UserId = 2,
            SessionId = 3,
            TotalAmount = 100m,
            BookingDate = DateTime.UtcNow,
            Status = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        #region GetAll

        [Fact]
        public void GetAll_ReturnsOk()
        {
            var list = new List<Booking> { CreateValidBooking() };
            _mockService.Setup(s => s.GetAllBookings()).Returns(list);

            var result = _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<BookingResponseDto>>(result.Value);
            Assert.Single(value);
            Assert.Equal(list[0].Id, value.First().Id);
        }

        [Fact]
        public void GetAll_ThrowsInvalidOperation_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetAllBookings())
                .Throws(new InvalidOperationException("No bookings found."));

            var result = _controller.GetAll() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No bookings found", result.Value.ToString());
        }

        [Fact]
        public void GetAll_ThrowsUnexpectedException_ReturnsBadRequest()
        {
            _mockService.Setup(s => s.GetAllBookings())
                .Throws(new Exception("Unexpected error."));

            var result = _controller.GetAll() as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected error", result.Value.ToString());
        }

        #endregion

        #region GetByUser

        [Fact]
        public void GetByUser_ReturnsOk()
        {
            var userId = 2;
            var list = new List<Booking> { CreateValidBooking() };
            _mockService.Setup(s => s.GetAllBookingsByUserId(userId)).Returns(list);

            var result = _controller.GetByUser(userId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<BookingResponseDto>>(result.Value);
            Assert.Single(value);
            Assert.Equal(userId, value.First().UserId);
        }

        [Fact]
        public void GetByUser_InvalidUserId_ReturnsBadRequest()
        {
            var userId = -1;
            _mockService.Setup(s => s.GetAllBookingsByUserId(userId))
                .Throws(new ArgumentOutOfRangeException("userId", "Invalid ID."));

            var result = _controller.GetByUser(userId) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid ID", result.Value.ToString());
        }

        [Fact]
        public void GetByUser_NoBookings_ReturnsNotFound()
        {
            var userId = 99;
            _mockService.Setup(s => s.GetAllBookingsByUserId(userId))
                .Throws(new InvalidOperationException("No bookings found for this user."));

            var result = _controller.GetByUser(userId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No bookings found for this user", result.Value.ToString());
        }

        [Fact]
        public void GetByUser_UnexpectedError_ReturnsBadRequest()
        {
            var userId = 2;
            _mockService.Setup(s => s.GetAllBookingsByUserId(userId))
                .Throws(new Exception("Unexpected error."));

            var result = _controller.GetByUser(userId) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected error", result.Value.ToString());
        }

        #endregion

        #region GetById

        [Fact]
        public void GetById_ReturnsOk()
        {
            var booking = CreateValidBooking();
            _mockService.Setup(s => s.GetBookingById(booking.Id)).Returns(booking);

            var result = _controller.GetById(booking.Id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsType<BookingResponseDto>(result.Value);
            Assert.Equal(booking.Id, value.Id);
        }

        [Fact]
        public void GetById_InvalidId_ReturnsBadRequest()
        {
            _mockService.Setup(s => s.GetBookingById(-1))
                .Throws(new ArgumentOutOfRangeException("id", "Invalid ID."));

            var result = _controller.GetById(-1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid ID", result.Value.ToString());
        }

        [Fact]
        public void GetById_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetBookingById(42))
                .Throws(new KeyNotFoundException("Booking not found."));

            var result = _controller.GetById(42) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Booking not found", result.Value.ToString());
        }

        [Fact]
        public void GetById_UnexpectedError_ReturnsBadRequest()
        {
            _mockService.Setup(s => s.GetBookingById(1))
                .Throws(new Exception("Unexpected error."));

            var result = _controller.GetById(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected error", result.Value.ToString());
        }

        #endregion

        #region Create

        [Fact]
        public void Create_ReturnsOk()
        {
            var dto = new BookingCreateDto { UserId = 2, SessionId = 3, SeatIds = new List<int> { 1, 2 } };
            var booking = CreateValidBooking();
            _mockService.Setup(s => s.CreateBookingWithSeats(It.IsAny<Booking>(), It.IsAny<IEnumerable<int>>())).Returns(booking);

            var result = _controller.Create(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsType<BookingResponseDto>(result.Value);
            Assert.Equal(booking.Id, value.Id);
        }

        [Fact]
        public void Create_NullBooking_ReturnsBadRequest()
        {
            var result = _controller.Create(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Create_ModelStateInvalid_ReturnsBadRequest()
        {
            var dto = new BookingCreateDto { UserId = 2, SessionId = 3, SeatIds = new List<int> { 1, 2 } };
            _controller.ModelState.AddModelError("SeatIds", "Required");

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.IsType<SerializableError>(result.Value);
        }

        [Fact]
        public void Create_ServiceThrowsArgumentNullException_ReturnsBadRequest()
        {
            var dto = new BookingCreateDto
            {
                UserId = 2,
                SessionId = 3,
                SeatIds = new List<int> { 1 }
            };

            _mockService
                .Setup(s => s.CreateBookingWithSeats(It.IsAny<Booking>(), It.IsAny<IEnumerable<int>>()))
                .Throws(new ArgumentNullException("newBooking", "Booking cannot be null."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Booking cannot be null", result.Value.ToString());
        }

        [Fact]
        public void Create_ServiceThrowsDbUpdateException_ReturnsBadRequest()
        {
            var dto = new BookingCreateDto
            {
                UserId = 2,
                SessionId = 3,
                SeatIds = new List<int> { 1 }
            };

            _mockService
                .Setup(s => s.CreateBookingWithSeats(It.IsAny<Booking>(), It.IsAny<IEnumerable<int>>()))
                .Throws(new DbUpdateException("Database error"));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Database error", result.Value.ToString());
        }

        [Fact]
        public void Create_ServiceThrowsUnexpectedException_ReturnsBadRequest()
        {
            var dto = new BookingCreateDto
            {
                UserId = 2,
                SessionId = 3,
                SeatIds = new List<int> { 1 }
            };

            _mockService
                .Setup(s => s.CreateBookingWithSeats(It.IsAny<Booking>(), It.IsAny<IEnumerable<int>>()))
                .Throws(new Exception("Unexpected failure"));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected failure", result.Value.ToString());
        }

        #endregion

        #region Update

        [Fact]
        public void Update_ReturnsOk()
        {
            var dto = new BookingUpdateDto { Id = 1, UserId = 2, SessionId = 3, TotalAmount = 200m, Status = true };
            var booking = CreateValidBooking();
            _mockService.Setup(s => s.UpdateBooking(dto.Id, It.IsAny<Booking>())).Returns(booking);

            var result = _controller.Update(dto.Id, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsType<BookingResponseDto>(result.Value);
            Assert.Equal(booking.Id, value.Id);
        }

        [Fact]
        public void Update_NullBooking_ReturnsBadRequest()
        {
            var result = _controller.Update(1, null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Update_IdMismatch_ReturnsBadRequest()
        {
            var dto = new BookingUpdateDto { Id = 1, UserId = 2, SessionId = 3, TotalAmount = 200m, Status = true };
            var result = _controller.Update(2, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("The provided ID does not match the booking ID", result!.Value!.ToString());
        }

        [Fact]
        public void Update_ServiceThrowsArgumentOutOfRangeException_ReturnsBadRequest()
        {
            var dto = new BookingUpdateDto
            {
                Id = 1,
                UserId = 2,
                SessionId = 3,
                TotalAmount = 100m,
                Status = true
            };

            _mockService
                .Setup(s => s.UpdateBooking(dto.Id, It.IsAny<Booking>()))
                .Throws(new ArgumentOutOfRangeException("id", "ID must be greater than zero."));

            var result = _controller.Update(dto.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater than zero", result.Value.ToString());
        }

        [Fact]
        public void Update_ServiceThrowsArgumentNullException_ReturnsBadRequest()
        {
            var dto = new BookingUpdateDto
            {
                Id = 1,
                UserId = 2,
                SessionId = 3,
                TotalAmount = 100m,
                Status = true
            };

            _mockService
                .Setup(s => s.UpdateBooking(dto.Id, It.IsAny<Booking>()))
                .Throws(new ArgumentNullException("updatedBooking", "Booking data cannot be null."));

            var result = _controller.Update(dto.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("cannot be null", result.Value.ToString());
        }

        [Fact]
        public void Update_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
        {
            var dto = new BookingUpdateDto
            {
                Id = 1,
                UserId = 2,
                SessionId = 3,
                TotalAmount = 100m,
                Status = true
            };

            _mockService
                .Setup(s => s.UpdateBooking(dto.Id, It.IsAny<Booking>()))
                .Throws(new KeyNotFoundException("Booking not found."));

            var result = _controller.Update(dto.Id, dto) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Booking not found", result.Value.ToString());
        }

        [Fact]
        public void Update_ServiceThrowsDbUpdateException_ReturnsBadRequest()
        {
            var dto = new BookingUpdateDto
            {
                Id = 1,
                UserId = 2,
                SessionId = 3,
                TotalAmount = 100m,
                Status = true
            };

            _mockService
                .Setup(s => s.UpdateBooking(dto.Id, It.IsAny<Booking>()))
                .Throws(new DbUpdateException("Database update failed."));

            var result = _controller.Update(dto.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Database update failed", result.Value.ToString());
        }

        [Fact]
        public void Update_ServiceThrowsUnexpectedException_ReturnsBadRequest()
        {
            var dto = new BookingUpdateDto
            {
                Id = 1,
                UserId = 2,
                SessionId = 3,
                TotalAmount = 100m,
                Status = true
            };

            _mockService
                .Setup(s => s.UpdateBooking(dto.Id, It.IsAny<Booking>()))
                .Throws(new Exception("Unexpected failure."));

            var result = _controller.Update(dto.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected failure", result.Value.ToString());
        }

        #endregion
    }
}
