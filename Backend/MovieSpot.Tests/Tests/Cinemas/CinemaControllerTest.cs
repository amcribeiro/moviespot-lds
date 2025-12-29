using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Cinemas;
using Xunit;
using static MovieSpot.DTO_s.CinemaDTO;

namespace MovieSpot.Tests.Controllers.Cinemas
{
    public class CinemaControllerTest
    {
        private readonly Mock<ICinemaService> _mockService;
        private readonly Mock<ILogger<CinemasController>> _mockLogger;
        private readonly CinemasController _controller;

        public CinemaControllerTest()
        {
            _mockService = new Mock<ICinemaService>();
            _mockLogger = new Mock<ILogger<CinemasController>>();
            _controller = new CinemasController(_mockService.Object, _mockLogger.Object);
        }

        private Cinema CreateCinema() => new Cinema
        {
            Id = 1,
            Name = "CineWorld",
            Street = "Main St",
            City = "Lisboa",
            State = "Lisboa",
            ZipCode = "1000-001",
            Country = "Portugal",
            Latitude = 38.7169m,
            Longitude = -9.1399m,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        #region GetCinemas

        [Fact]
        public void GetCinemas_ReturnsOk_WhenCinemasExist()
        {
            var cinemas = new List<Cinema> { CreateCinema() };
            _mockService.Setup(s => s.GetAllCinemas()).Returns(cinemas);

            var result = _controller.GetCinemas().Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsAssignableFrom<IEnumerable<CinemaResponseDto>>(result.Value);
            Assert.Single(value);
            Assert.Equal("CineWorld", value.First().Name);
        }

        [Fact]
        public void GetCinemas_ReturnsNotFound_WhenServiceThrowsInvalidOperation()
        {
            _mockService.Setup(s => s.GetAllCinemas())
                .Throws(new InvalidOperationException("No cinemas available."));

            var result = _controller.GetCinemas().Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No cinemas available", result.Value.ToString());
        }

        #endregion

        #region GetCinema

        [Fact]
        public void GetCinema_ReturnsOk_WhenValidId()
        {
            var cinema = CreateCinema();
            _mockService.Setup(s => s.GetCinemaById(1)).Returns(cinema);

            var result = _controller.GetCinema(1).Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var dto = Assert.IsType<CinemaResponseDto>(result.Value);
            Assert.Equal("CineWorld", dto.Name);
            Assert.Equal("Lisboa", dto.City);
        }

        [Fact]
        public void GetCinema_ReturnsBadRequest_WhenInvalidId()
        {
            _mockService.Setup(s => s.GetCinemaById(0))
                .Throws(new ArgumentOutOfRangeException("id", "ID must be greater than zero."));

            var result = _controller.GetCinema(0).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater than zero", result.Value.ToString());
        }

        [Fact]
        public void GetCinema_ReturnsNotFound_WhenKeyNotFound()
        {
            _mockService.Setup(s => s.GetCinemaById(1))
                .Throws(new KeyNotFoundException("Cinema not found."));

            var result = _controller.GetCinema(1).Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Cinema not found", result.Value.ToString());
        }

        #endregion


        #region AddCinema

        [Fact]
        public void AddCinema_ReturnsCreated_WhenValidRequest()
        {
            var dto = new CinemaCreateDto
            {
                Name = "Novo Cinema",
                Street = "Rua da Alegria",
                City = "Porto",
                Country = "Portugal",
                Latitude = 41.15m,
                Longitude = -8.61m
            };

            var result = _controller.AddCinema(dto).Result as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("GetCinema", result.ActionName);

            var response = Assert.IsType<CinemaResponseDto>(result.Value);
            Assert.Equal("Novo Cinema", response.Name);
            Assert.Equal("Porto", response.City);
        }

        [Fact]
        public void AddCinema_ReturnsValidationProblem_WhenModelStateInvalid()
        {
            var dto = new CinemaCreateDto { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            var actionResult = _controller.AddCinema(dto);
            var objectResult = actionResult.Result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.IsType<ValidationProblemDetails>(objectResult.Value);

            var details = objectResult.Value as ValidationProblemDetails;
            Assert.NotNull(details);
            Assert.Contains("Name", details.Errors.Keys);
        }

        [Fact]
        public void AddCinema_ReturnsBadRequest_WhenServiceThrowsArgumentNullException()
        {
            var dto = new CinemaCreateDto
            {
                Name = "Cinema Zero",
                Street = "Rua X",
                City = "Lisboa",
                Country = "Portugal"
            };

            _mockService.Setup(s => s.AddCinema(It.IsAny<Cinema>()))
                .Throws(new ArgumentNullException("cinema", "Cinema cannot be null."));

            var result = _controller.AddCinema(dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Cinema cannot be null", result.Value.ToString());
        }

        [Fact]
        public void AddCinema_ReturnsConflict_WhenDbUpdateExceptionThrown()
        {
            var dto = new CinemaCreateDto
            {
                Name = "Cinema Crash",
                Street = "Rua Falha",
                City = "Lisboa",
                Country = "Portugal"
            };

            _mockService.Setup(s => s.AddCinema(It.IsAny<Cinema>()))
                .Throws(new DbUpdateException("Database insert failed."));

            var result = _controller.AddCinema(dto).Result as ConflictObjectResult;

            Assert.NotNull(result);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("Failed to create cinema record", result.Value.ToString());
        }

        #endregion

        #region UpdateCinema

        [Fact]
        public void UpdateCinema_ReturnsOk_WhenValidRequest()
        {
            var existing = CreateCinema();
            var dto = new CinemaUpdateDto
            {
                Id = 1,
                Name = "Cine Updated",
                Street = "Rua Nova",
                City = "Porto",
                Country = "Portugal",
                Latitude = 41.15m,
                Longitude = -8.61m
            };

            _mockService.Setup(s => s.GetCinemaById(1)).Returns(existing);

            var result = _controller.UpdateCinema(1, dto).Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = Assert.IsType<CinemaResponseDto>(result.Value);
            Assert.Equal("Cine Updated", response.Name);
            Assert.Equal("Porto", response.City);
        }

        [Fact]
        public void UpdateCinema_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var dto = new CinemaUpdateDto { Id = 1 };
            _controller.ModelState.AddModelError("Name", "Required");

            var actionResult = _controller.UpdateCinema(1, dto);
            var objectResult = actionResult.Result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.IsType<ValidationProblemDetails>(objectResult.Value);

            var details = objectResult.Value as ValidationProblemDetails;
            Assert.NotNull(details);
            Assert.Contains("Name", details.Errors.Keys);
        }

        [Fact]
        public void UpdateCinema_ReturnsBadRequest_WhenIdMismatch()
        {
            var dto = new CinemaUpdateDto { Id = 2, Name = "CineX" };

            var result = _controller.UpdateCinema(1, dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Path ID does not match", result.Value.ToString());
        }

        [Fact]
        public void UpdateCinema_ReturnsBadRequest_WhenArgumentNullExceptionThrown()
        {
            var dto = new CinemaUpdateDto
            {
                Id = 1,
                Name = "Cine Bug",
                Street = "Rua Teste",
                City = "Lisboa",
                Country = "Portugal"
            };

            _mockService.Setup(s => s.GetCinemaById(1)).Throws(new ArgumentNullException("cinema", "Cinema is null."));

            var result = _controller.UpdateCinema(1, dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Cinema is null", result.Value.ToString());
        }

        [Fact]
        public void UpdateCinema_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
        {
            var dto = new CinemaUpdateDto
            {
                Id = 1,
                Name = "Cine Ghost",
                Street = "Rua X",
                City = "Lisboa",
                Country = "Portugal"
            };

            _mockService.Setup(s => s.GetCinemaById(1))
                .Throws(new KeyNotFoundException("Cinema not found."));

            var result = _controller.UpdateCinema(1, dto).Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Cinema not found", result.Value.ToString());
        }

        [Fact]
        public void UpdateCinema_ReturnsConflict_WhenDbUpdateExceptionThrown()
        {
            var dto = new CinemaUpdateDto
            {
                Id = 1,
                Name = "Cine Conflict",
                Street = "Rua Y",
                City = "Lisboa",
                Country = "Portugal"
            };

            _mockService.Setup(s => s.GetCinemaById(1)).Returns(CreateCinema());
            _mockService.Setup(s => s.UpdateCinema(It.IsAny<Cinema>()))
                .Throws(new DbUpdateException("Database update failed."));

            var result = _controller.UpdateCinema(1, dto).Result as ConflictObjectResult;

            Assert.NotNull(result);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("Failed to update cinema record", result.Value.ToString());
        }

        #endregion


        #region DeleteCinema

        [Fact]
        public void DeleteCinema_ReturnsNoContent_WhenSuccessful()
        {
            _mockService.Setup(s => s.RemoveCinema(1));

            var result = _controller.DeleteCinema(1) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }

        [Fact]
        public void DeleteCinema_ReturnsBadRequest_WhenInvalidId()
        {
            _mockService.Setup(s => s.RemoveCinema(0))
                .Throws(new ArgumentOutOfRangeException("id", "ID must be greater than zero."));

            var result = _controller.DeleteCinema(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater than zero", result.Value.ToString());
        }

        [Fact]
        public void DeleteCinema_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
        {
            _mockService.Setup(s => s.RemoveCinema(1))
                .Throws(new KeyNotFoundException("Cinema not found."));

            var result = _controller.DeleteCinema(1) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Cinema not found", result.Value.ToString());
        }

        [Fact]
        public void DeleteCinema_ReturnsConflict_WhenDbUpdateExceptionThrown()
        {
            _mockService.Setup(s => s.RemoveCinema(1))
                .Throws(new DbUpdateException("Database delete failed."));

            var result = _controller.DeleteCinema(1) as ConflictObjectResult;

            Assert.NotNull(result);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("Failed to delete cinema record", result.Value.ToString());
        }

        #endregion
    }
}
