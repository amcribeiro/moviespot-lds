using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.Models;
using MovieSpot.Services.CinemaHalls;
using Xunit;
using static MovieSpot.DTO_s.CinemaHallDTO;

namespace MovieSpot.Tests.Controllers.CinemaHalls
{
    public class CinemaHallControllerTest
    {
        private readonly Mock<ICinemaHallService> _mockService;
        private readonly CinemaHallController _controller;

        public CinemaHallControllerTest()
        {
            _mockService = new Mock<ICinemaHallService>();
            _controller = new CinemaHallController(_mockService.Object);
        }

        private CinemaHall CreateHall()
        {
            return new CinemaHall
            {
                Id = 1,
                Name = "Sala Principal",
                CinemaId = 10
            };
        }

        [Fact]
        public void GetAll_ReturnsOk_WhenHallsExist()
        {
            var list = new List<CinemaHall> { CreateHall() };
            _mockService.Setup(s => s.GetAllCinemaHalls()).Returns(list);

            var result = _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var value = Assert.IsAssignableFrom<IEnumerable<CinemaHallReadDto>>(result.Value);
            Assert.Single(value);
        }

        [Fact]
        public void GetAll_ReturnsNotFound_WhenInvalidOperation()
        {
            _mockService.Setup(s => s.GetAllCinemaHalls())
                .Throws(new InvalidOperationException("No cinema halls available."));

            var result = _controller.GetAll() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No cinema halls", result.Value!.ToString());
        }

        [Fact]
        public void GetAll_ReturnsBadRequest_OnUnexpectedException()
        {
            _mockService.Setup(s => s.GetAllCinemaHalls())
                .Throws(new Exception("Unexpected failure."));

            var result = _controller.GetAll() as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected failure", result.Value!.ToString());
        }

        [Fact]
        public void GetById_ReturnsOk_WhenValidId()
        {
            var hall = CreateHall();
            _mockService.Setup(s => s.GetCinemaHallById(1)).Returns(hall);

            var result = _controller.GetById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var value = Assert.IsType<CinemaHallDetailsDto>(result.Value);
            Assert.Equal("Sala Principal", value.Name);
        }

        [Fact]
        public void GetById_ReturnsBadRequest_WhenInvalidId()
        {
            _mockService.Setup(s => s.GetCinemaHallById(0))
                .Throws(new ArgumentOutOfRangeException("id", "Invalid ID."));

            var result = _controller.GetById(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid ID", result.Value!.ToString());
        }

        [Fact]
        public void GetById_ReturnsNotFound_WhenKeyNotFound()
        {
            _mockService.Setup(s => s.GetCinemaHallById(999))
                .Throws(new KeyNotFoundException("Cinema hall not found."));

            var result = _controller.GetById(999) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Cinema hall not found", result.Value!.ToString());
        }

        [Fact]
        public void GetById_ReturnsBadRequest_OnUnexpectedException()
        {
            _mockService.Setup(s => s.GetCinemaHallById(1))
                .Throws(new Exception("Unexpected error."));

            var result = _controller.GetById(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected error", result.Value!.ToString());
        }

        [Fact]
        public void GetByCinemaId_ReturnsOk_WhenValid()
        {
            var halls = new List<CinemaHall> { CreateHall() };
            _mockService.Setup(s => s.GetCinemaHallsByCinemaId(10)).Returns(halls);

            var result = _controller.GetByCinemaId(10) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var value = Assert.IsAssignableFrom<IEnumerable<CinemaHallReadDto>>(result.Value);
            Assert.Single(value);
        }

        [Fact]
        public void GetByCinemaId_ReturnsBadRequest_WhenInvalidId()
        {
            _mockService.Setup(s => s.GetCinemaHallsByCinemaId(0))
                .Throws(new ArgumentOutOfRangeException("cinemaId", "Invalid ID."));

            var result = _controller.GetByCinemaId(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid ID", result.Value!.ToString());
        }

        [Fact]
        public void GetByCinemaId_ReturnsNotFound_WhenNoHalls()
        {
            _mockService.Setup(s => s.GetCinemaHallsByCinemaId(999))
                .Throws(new KeyNotFoundException("No cinema halls found."));

            var result = _controller.GetByCinemaId(999) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No cinema halls found", result.Value!.ToString());
        }

        [Fact]
        public void GetByCinemaId_ReturnsBadRequest_OnUnexpectedException()
        {
            _mockService.Setup(s => s.GetCinemaHallsByCinemaId(10))
                .Throws(new Exception("Unexpected failure."));

            var result = _controller.GetByCinemaId(10) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected failure", result.Value!.ToString());
        }

        [Fact]
        public void Create_ReturnsOk_WhenValid()
        {
            var hall = CreateHall();
            var dto = new CinemaHallCreateDto { Name = hall.Name, CinemaId = hall.CinemaId };

            _mockService.Setup(s => s.AddCinemaHall(It.IsAny<CinemaHall>())).Returns(hall);

            var result = _controller.Create(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var value = Assert.IsType<CinemaHallReadDto>(result.Value);
            Assert.Equal("Sala Principal", value.Name);
        }

        [Fact]
        public void Create_ReturnsBadRequest_WhenNull()
        {
            var result = _controller.Create(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Cinema hall data is required.", result.Value!.ToString());
        }

        [Fact]
        public void Create_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var dto = new CinemaHallCreateDto { Name = "", CinemaId = 10 };
            _controller.ModelState.AddModelError("Name", "Requerido");

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.IsType<SerializableError>(result.Value);
        }

        [Fact]
        public void Create_ReturnsBadRequest_WhenArgumentNullException()
        {
            var dto = new CinemaHallCreateDto { Name = "Sala Teste", CinemaId = 1 };
            _mockService.Setup(s => s.AddCinemaHall(It.IsAny<CinemaHall>()))
                .Throws(new ArgumentNullException("newCinemaHall", "Invalid cinema hall."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid cinema hall", result.Value!.ToString());
        }

        [Fact]
        public void Create_ReturnsBadRequest_WhenDbUpdateException()
        {
            var dto = new CinemaHallCreateDto { Name = "Sala Teste", CinemaId = 1 };
            _mockService.Setup(s => s.AddCinemaHall(It.IsAny<CinemaHall>()))
                .Throws(new DbUpdateException("Database error."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Database error", result.Value!.ToString());
        }

        [Fact]
        public void Create_ReturnsBadRequest_OnUnexpectedException()
        {
            var dto = new CinemaHallCreateDto { Name = "Sala Teste", CinemaId = 1 };
            _mockService.Setup(s => s.AddCinemaHall(It.IsAny<CinemaHall>()))
                .Throws(new Exception("Unexpected error."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected error", result.Value!.ToString());
        }

        [Fact]
        public void Update_ReturnsOk_WhenValid()
        {
            var hall = CreateHall();
            var dto = new CinemaHallUpdateDto { Id = 1, Name = hall.Name, CinemaId = hall.CinemaId };
            _mockService.Setup(s => s.UpdateCinemaHall(1, It.IsAny<CinemaHall>())).Returns(hall);

            var result = _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void Update_ReturnsBadRequest_WhenNull()
        {
            var result = _controller.Update(1, null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var dto = new CinemaHallUpdateDto { Id = 2, Name = "Sala", CinemaId = 10 };
            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("does not match the cinema hall ID", result.Value!.ToString());
        }

        [Fact]
        public void Update_ReturnsBadRequest_WhenArgumentOutOfRange()
        {
            var dto = new CinemaHallUpdateDto { Id = 1, Name = "Sala", CinemaId = 10 };
            _mockService.Setup(s => s.UpdateCinemaHall(1, It.IsAny<CinemaHall>()))
                .Throws(new ArgumentOutOfRangeException("id", "Invalid."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Update_ReturnsNotFound_WhenKeyNotFound()
        {
            var dto = new CinemaHallUpdateDto { Id = 1, Name = "Sala", CinemaId = 10 };
            _mockService.Setup(s => s.UpdateCinemaHall(1, It.IsAny<CinemaHall>()))
                .Throws(new KeyNotFoundException("Not found."));

            var result = _controller.Update(1, dto) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void Update_ReturnsBadRequest_WhenDbUpdateException()
        {
            var dto = new CinemaHallUpdateDto { Id = 1, Name = "Sala", CinemaId = 10 };
            _mockService.Setup(s => s.UpdateCinemaHall(1, It.IsAny<CinemaHall>()))
                .Throws(new DbUpdateException("Database error."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Update_ReturnsBadRequest_OnUnexpectedException()
        {
            var dto = new CinemaHallUpdateDto { Id = 1, Name = "Sala", CinemaId = 10 };
            _mockService.Setup(s => s.UpdateCinemaHall(1, It.IsAny<CinemaHall>()))
                .Throws(new Exception("Unexpected failure."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected failure", result.Value!.ToString());
        }

        [Fact]
        public void Update_ReturnsBadRequest_WhenArgumentNullException()
        {
            var dto = new CinemaHallUpdateDto { Id = 1, Name = "Sala Teste", CinemaId = 2 };
            _mockService
                .Setup(s => s.UpdateCinemaHall(1, It.IsAny<CinemaHall>()))
                .Throws(new ArgumentNullException("updatedCinemaHall", "Invalid data."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid data", result.Value!.ToString());
        }

        [Fact]
        public void Delete_ReturnsOk_WhenValid()
        {
            var result = _controller.Delete(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("successfully deleted", result.Value!.ToString());
        }

        [Fact]
        public void Delete_ReturnsBadRequest_WhenInvalidId()
        {
            _mockService.Setup(s => s.RemoveCinemaHall(0))
                .Throws(new ArgumentOutOfRangeException("id", "Invalid."));

            var result = _controller.Delete(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Delete_ReturnsNotFound_WhenKeyNotFound()
        {
            _mockService.Setup(s => s.RemoveCinemaHall(99))
                .Throws(new KeyNotFoundException("Not found."));

            var result = _controller.Delete(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void Delete_ReturnsBadRequest_WhenDbUpdateException()
        {
            _mockService.Setup(s => s.RemoveCinemaHall(1))
                .Throws(new DbUpdateException("Database error."));

            var result = _controller.Delete(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Delete_ReturnsBadRequest_OnUnexpectedException()
        {
            _mockService.Setup(s => s.RemoveCinemaHall(1))
                .Throws(new Exception("Unexpected failure."));

            var result = _controller.Delete(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Unexpected failure", result.Value!.ToString());
        }
    }
}
