using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Movies;
using Xunit;

namespace MovieSpot.Tests.Controllers.Movies
{
    public class MovieControllerTest
    {
        private readonly Mock<IMovieService> _mockService;
        private readonly Mock<ILogger<MovieController>> _mockLogger;
        private readonly MovieController _controller;

        public MovieControllerTest()
        {
            _mockService = new Mock<IMovieService>();
            _mockLogger = new Mock<ILogger<MovieController>>();
            _controller = new MovieController(_mockService.Object, _mockLogger.Object);
        }

        private Movie CreateMovie()
        {
            return new Movie
            {
                Id = 1,
                Title = "Inception",
                Description = "Mind-bending movie",
                Duration = 120,
                Language = "English",
                ReleaseDate = new DateTime(2010, 7, 16),
                Country = "USA",
                PosterPath = "poster.jpg",
                MovieGenres = new List<MovieGenre>
                {
                    new MovieGenre { Genre = new Genre { Name = "Sci-Fi" } }
                }
            };
        }

        #region GetAllMovies

        [Fact]
        public void GetAllMovies_ReturnsOk_WhenMoviesExist()
        {
            var list = new List<Movie> { CreateMovie() };
            _mockService.Setup(s => s.GetMovies()).Returns(list);

            var result = _controller.GetAllMovies().Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = Assert.IsAssignableFrom<List<MovieDto>>(result.Value);
            Assert.Single(value);
            Assert.Equal("Inception", value.First().Title);
        }

        [Fact]
        public void GetAllMovies_ReturnsNotFound_WhenNoMoviesExist()
        {
            _mockService.Setup(s => s.GetMovies()).Returns(new List<Movie>());

            var result = _controller.GetAllMovies().Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No movies found", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllMovies_ReturnsInternalServerError_OnException()
        {
            _mockService.Setup(s => s.GetMovies()).Throws(new Exception("DB error"));

            var result = _controller.GetAllMovies().Result as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("error", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region GetMovieById

        [Fact]
        public void GetMovieById_ReturnsOk_WhenValidId()
        {
            var movie = CreateMovie();
            _mockService.Setup(s => s.GetMovie(1)).Returns(movie);

            var result = _controller.GetMovieById(1).Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var dto = Assert.IsType<MovieDto>(result.Value);
            Assert.Equal("Inception", dto.Title);
        }

        [Fact]
        public void GetMovieById_ReturnsBadRequest_WhenInvalidId()
        {
            var result = _controller.GetMovieById(0).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetMovieById_ReturnsNotFound_WhenMovieIsNull()
        {
            _mockService.Setup(s => s.GetMovie(1)).Returns((Movie)null!);

            var result = _controller.GetMovieById(1).Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("not found", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetMovieById_ReturnsNotFound_WhenKeyNotFoundException()
        {
            _mockService.Setup(s => s.GetMovie(1)).Throws(new KeyNotFoundException("Movie not found."));

            var result = _controller.GetMovieById(1).Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Movie with ID 1", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetMovieById_ReturnsInternalServerError_OnUnexpectedError()
        {
            _mockService.Setup(s => s.GetMovie(1)).Throws(new Exception("Unexpected fail."));

            var result = _controller.GetMovieById(1).Result as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("error", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region AddMovie

        [Fact]
        public void AddMovie_ReturnsCreated_WhenValidMovie()
        {
            var movie = CreateMovie();
            _mockService.Setup(s => s.AddMovie(movie));

            var result = _controller.AddMovie(movie) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("GetMovieById", result.ActionName);
            Assert.Equal(movie, result.Value);
        }

        [Fact]
        public void AddMovie_ReturnsBadRequest_WhenNullMovie()
        {
            var result = _controller.AddMovie(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("required", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddMovie_ReturnsInternalServerError_OnException()
        {
            var movie = CreateMovie();
            _mockService.Setup(s => s.AddMovie(It.IsAny<Movie>())).Throws(new Exception("Insert failed"));

            var result = _controller.AddMovie(movie) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("error", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region RemoveMovie

        [Fact]
        public void RemoveMovie_ReturnsNoContent_WhenSuccessful()
        {
            _mockService.Setup(s => s.RemoveMovie(1));

            var result = _controller.RemoveMovie(1) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }

        [Fact]
        public void RemoveMovie_ReturnsBadRequest_WhenInvalidId()
        {
            var result = _controller.RemoveMovie(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("greater", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RemoveMovie_ReturnsNotFound_WhenKeyNotFound()
        {
            _mockService.Setup(s => s.RemoveMovie(1)).Throws(new KeyNotFoundException("Movie not found."));

            var result = _controller.RemoveMovie(1) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("not found", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RemoveMovie_ReturnsInternalServerError_OnUnexpectedError()
        {
            _mockService.Setup(s => s.RemoveMovie(1)).Throws(new Exception("Delete failed."));

            var result = _controller.RemoveMovie(1) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("error", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
