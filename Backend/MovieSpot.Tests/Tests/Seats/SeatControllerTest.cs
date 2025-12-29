using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Seats;

namespace MovieSpot.Tests.Controllers.Seats
{
    public class SeatControllerTest
    {
        private readonly Mock<ISeatService> _seatServiceMock;
        private readonly Mock<ILogger<SeatController>> _loggerMock;
        private readonly SeatController _controller;

        public SeatControllerTest()
        {
            _seatServiceMock = new Mock<ISeatService>();
            _loggerMock = new Mock<ILogger<SeatController>>();
            _controller = new SeatController(_seatServiceMock.Object, _loggerMock.Object);
        }

        private Seat BuildSeat(int id = 1, int hallId = 10, string number = "A1", string type = "Normal")
        {
            return new Seat
            {
                Id = id,
                CinemaHallId = hallId,
                CinemaHall = new CinemaHall { Id = hallId, Name = $"Hall {hallId}" },
                SeatNumber = number,
                SeatType = type,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
        }

        #region GetAllSeats

        [Fact]
        public async Task GetAllSeats_WhenSeatsExist_ReturnsOk()
        {
            var seats = new List<Seat> { BuildSeat(1), BuildSeat(2, 10, "A2") };

            _seatServiceMock.Setup(s => s.GetAllSeatsAsync()).ReturnsAsync(seats);

            var result = await _controller.GetAllSeats();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<SeatDTO.SeatResponseDto>>(ok.Value);
            Assert.Equal(2, data.Count());
            Assert.Contains(data, s => s.SeatNumber == "A2");
        }

        [Fact]
        public async Task GetAllSeats_WhenEmpty_ReturnsNotFound()
        {
            _seatServiceMock.Setup(s => s.GetAllSeatsAsync()).ReturnsAsync(new List<Seat>());

            var result = await _controller.GetAllSeats();

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No seats found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetAllSeats_WhenException_ReturnsProblem500()
        {
            _seatServiceMock.Setup(s => s.GetAllSeatsAsync()).ThrowsAsync(new Exception("boom"));

            var result = await _controller.GetAllSeats();

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        #endregion

        #region GetSeatById

        [Fact]
        public async Task GetSeatById_Valid_ReturnsOk()
        {
            var seat = BuildSeat(5);
            _seatServiceMock.Setup(s => s.GetSeatByIdAsync(5)).ReturnsAsync(seat);

            var result = await _controller.GetSeatById(5);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<SeatDTO.SeatResponseDto>(ok.Value);
            Assert.Equal("A1", dto.SeatNumber);
            Assert.Equal(5, dto.Id);
        }

        [Fact]
        public async Task GetSeatById_NotFound_Returns404()
        {
            _seatServiceMock.Setup(s => s.GetSeatByIdAsync(9)).ReturnsAsync((Seat?)null);

            var result = await _controller.GetSeatById(9);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetSeatById_WhenException_Returns500()
        {
            _seatServiceMock.Setup(s => s.GetSeatByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB Error"));

            var result = await _controller.GetSeatById(1);

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        #endregion

        #region GetSeatsByCinemaHall

        [Fact]
        public async Task GetSeatsByCinemaHall_Valid_ReturnsOk()
        {
            var seats = new List<Seat> { BuildSeat(1, 2, "C1") };

            _seatServiceMock.Setup(s => s.GetSeatsByCinemaHallIdAsync(2)).ReturnsAsync(seats);

            var result = await _controller.GetSeatsByCinemaHall(2);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsAssignableFrom<IEnumerable<SeatDTO.SeatResponseDto>>(ok.Value);
            Assert.Single(dto);
        }

        [Fact]
        public async Task GetSeatsByCinemaHall_Empty_ReturnsNotFound()
        {
            _seatServiceMock.Setup(s => s.GetSeatsByCinemaHallIdAsync(3)).ReturnsAsync(new List<Seat>());

            var result = await _controller.GetSeatsByCinemaHall(3);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No seats found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetSeatsByCinemaHall_Exception_Returns500()
        {
            _seatServiceMock.Setup(s => s.GetSeatsByCinemaHallIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB exploded"));

            var result = await _controller.GetSeatsByCinemaHall(1);

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        #endregion

        #region AddSeat

        [Fact]
        public async Task AddSeat_Valid_ReturnsCreated()
        {
            var dto = new SeatDTO.SeatCreateDto
            {
                CinemaHallId = 1,
                SeatNumber = "A1",
                SeatType = "VIP"
            };

            var created = BuildSeat(10, 1, "A1", "VIP");

            _seatServiceMock.Setup(s => s.AddSeatAsync(It.IsAny<Seat>())).ReturnsAsync(created);

            var result = await _controller.AddSeat(dto);

            var createdRes = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<SeatDTO.SeatResponseDto>(createdRes.Value);
            Assert.Equal("A1", response.SeatNumber);
        }

        [Fact]
        public async Task AddSeat_InvalidModel_ReturnsValidationProblem()
        {
            _controller.ModelState.AddModelError("SeatNumber", "Required");
            var dto = new SeatDTO.SeatCreateDto();

            var result = await _controller.AddSeat(dto);

            Assert.IsType<ObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddSeat_ArgumentNull_ReturnsBadRequest()
        {
            _seatServiceMock.Setup(s => s.AddSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new ArgumentNullException("Seat is null"));

            var result = await _controller.AddSeat(new SeatDTO.SeatCreateDto());

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Seat is null", bad.Value.ToString());
        }

        [Fact]
        public async Task AddSeat_InvalidOperation_ReturnsConflict()
        {
            _seatServiceMock.Setup(s => s.AddSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new InvalidOperationException("Duplicate seat"));

            var result = await _controller.AddSeat(new SeatDTO.SeatCreateDto());

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("Duplicate seat", conflict.Value.ToString());
        }

        [Fact]
        public async Task AddSeat_DbUpdateException_ReturnsProblem500()
        {
            _seatServiceMock.Setup(s => s.AddSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new DbUpdateException("Database fail"));

            var result = await _controller.AddSeat(new SeatDTO.SeatCreateDto());

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);

            var details = Assert.IsType<ProblemDetails>(problem.Value);
            Assert.Equal("Database error", details.Title);
            Assert.Equal("Error while creating seat.", details.Detail);
        }


        [Fact]
        public async Task AddSeat_GenericException_ReturnsProblem500()
        {
            _seatServiceMock.Setup(s => s.AddSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var result = await _controller.AddSeat(new SeatDTO.SeatCreateDto());

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        #endregion

        #region UpdateSeat

        [Fact]
        public async Task UpdateSeat_Valid_ReturnsOk()
        {
            var dto = new SeatDTO.SeatUpdateDto
            {
                Id = 5,
                CinemaHallId = 2,
                SeatNumber = "D4",
                SeatType = "Normal"
            };

            var updated = BuildSeat(5, 2, "D4");

            _seatServiceMock.Setup(s => s.UpdateSeatAsync(It.IsAny<Seat>())).ReturnsAsync(updated);

            var result = await _controller.UpdateSeat(dto);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var res = Assert.IsType<SeatDTO.SeatResponseDto>(ok.Value);
            Assert.Equal("D4", res.SeatNumber);
        }

        [Fact]
        public async Task UpdateSeat_InvalidModel_ReturnsValidationProblem()
        {
            _controller.ModelState.AddModelError("SeatNumber", "Required");

            var dto = new SeatDTO.SeatUpdateDto();

            var result = await _controller.UpdateSeat(dto);

            Assert.IsType<ObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateSeat_KeyNotFound_ReturnsNotFound()
        {
            _seatServiceMock.Setup(s => s.UpdateSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new KeyNotFoundException("Seat not found"));

            var result = await _controller.UpdateSeat(new SeatDTO.SeatUpdateDto());

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("Seat not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task UpdateSeat_InvalidOperation_ReturnsConflict()
        {
            _seatServiceMock.Setup(s => s.UpdateSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new InvalidOperationException("Seat duplicate"));

            var result = await _controller.UpdateSeat(new SeatDTO.SeatUpdateDto());

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("Seat duplicate", conflict.Value.ToString());
        }

        [Fact]
        public async Task UpdateSeat_DbUpdateException_ReturnsProblem500()
        {
            _seatServiceMock.Setup(s => s.UpdateSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new DbUpdateException("DB fail"));

            var result = await _controller.UpdateSeat(new SeatDTO.SeatUpdateDto());

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        [Fact]
        public async Task UpdateSeat_GenericException_ReturnsProblem500()
        {
            _seatServiceMock.Setup(s => s.UpdateSeatAsync(It.IsAny<Seat>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var result = await _controller.UpdateSeat(new SeatDTO.SeatUpdateDto());

            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        #endregion

        #region DeleteSeat

        [Fact]
        public async Task DeleteSeat_Existing_ReturnsNoContent()
        {
            _seatServiceMock.Setup(s => s.RemoveSeatAsync(5)).ReturnsAsync(true);

            var result = await _controller.DeleteSeat(5);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteSeat_NotFound_Returns404()
        {
            _seatServiceMock.Setup(s => s.RemoveSeatAsync(5)).ReturnsAsync(false);

            var result = await _controller.DeleteSeat(5);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task DeleteSeat_Exception_ReturnsProblem500()
        {
            _seatServiceMock.Setup(s => s.RemoveSeatAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB crash"));

            var result = await _controller.DeleteSeat(1);

            var problem = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        #endregion
    }
}