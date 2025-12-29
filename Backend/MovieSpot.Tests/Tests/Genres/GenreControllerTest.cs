using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.Models;
using MovieSpot.Services.Genres;
using static MovieSpot.DTO_s.GenreDTO;

namespace MovieSpot.Tests.Controllers.Genres
{
    public class GenreControllerTest
    {
        private readonly Mock<IGenreService> _genreServiceMock;
        private readonly Mock<ILogger<GenreController>> _loggerMock;
        private readonly GenreController _controller;

        public GenreControllerTest()
        {
            _genreServiceMock = new Mock<IGenreService>();
            _loggerMock = new Mock<ILogger<GenreController>>();
            _controller = new GenreController(_genreServiceMock.Object, _loggerMock.Object);
        }

        #region GET /genre

        [Fact]
        public void GetGenres_ReturnsOk_WhenGenresExist()
        {
            var genres = new List<Genre>
            {
                new Genre { Id = 1, Name = "Action" },
                new Genre { Id = 2, Name = "Comedy" }
            };
            _genreServiceMock.Setup(s => s.GetAllGenresFromDb()).Returns(genres);

            var result = _controller.GetGenres();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGenres = Assert.IsAssignableFrom<List<GenreResponseDto>>(okResult.Value);
            Assert.Equal(2, returnedGenres.Count);
            Assert.Contains(returnedGenres, g => g.Name == "Action");
            Assert.Contains(returnedGenres, g => g.Name == "Comedy");
        }

        [Fact]
        public void GetGenres_ReturnsNotFound_WhenNoGenresExist()
        {
            _genreServiceMock.Setup(s => s.GetAllGenresFromDb()).Returns(new List<Genre>());

            var result = _controller.GetGenres();

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No genres found in the database.", notFound.Value);
        }

        [Fact]
        public void GetGenres_ReturnsInternalServerError_OnUnexpectedException()
        {
            _genreServiceMock.Setup(s => s.GetAllGenresFromDb()).Throws(new Exception("DB Error"));

            var result = _controller.GetGenres();

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("An error occurred while retrieving genres.", obj.Value);
        }

        [Fact]
        public void GetGenres_ReturnsNotFound_OnInvalidOperationException()
        {
            _genreServiceMock
                .Setup(s => s.GetAllGenresFromDb())
                .Throws(new InvalidOperationException());

            var result = _controller.GetGenres();

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No genres found in the database.", notFoundResult.Value);
        }

        #endregion

        #region GET /genre/{id}

        [Fact]
        public void GetGenre_ReturnsBadRequest_WhenIdIsInvalid()
        {
            var result = _controller.GetGenre(0);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Genre ID must be greater than zero.", badRequest.Value);
        }

        [Fact]
        public void GetGenre_ReturnsOk_WhenGenreExists()
        {
            var genre = new Genre { Id = 1, Name = "Action" };
            _genreServiceMock.Setup(s => s.GetGenreById(1)).Returns(genre);

            var result = _controller.GetGenre(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGenre = Assert.IsType<GenreResponseDto>(okResult.Value);
            Assert.Equal("Action", returnedGenre.Name);
        }

        [Fact]
        public void GetGenre_ReturnsNotFound_WhenGenreMissing()
        {
            _genreServiceMock.Setup(s => s.GetGenreById(99))
                .Throws(new KeyNotFoundException());

            var result = _controller.GetGenre(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Genre with ID 99 was not found.", notFound.Value);
        }

        [Fact]
        public void GetGenre_ReturnsInternalServerError_OnUnexpectedException()
        {
            _genreServiceMock.Setup(s => s.GetGenreById(1))
                .Throws(new Exception("Unknown error"));

            var result = _controller.GetGenre(1);

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("An error occurred while retrieving the genre.", obj.Value);
        }

        #endregion

        #region POST /genre/sync

        [Fact]
        public async Task SyncGenres_ReturnsNoContent_WhenSuccessful()
        {
            _genreServiceMock.Setup(s => s.SyncGenresAsync()).Returns(Task.CompletedTask);

            var result = await _controller.SyncGenres();

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SyncGenres_ReturnsInternalServerError_OnException()
        {
            _genreServiceMock.Setup(s => s.SyncGenresAsync())
                .ThrowsAsync(new Exception("API error"));

            var result = await _controller.SyncGenres();

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("An error occurred while synchronizing genres.", obj.Value);
        }

        #endregion
    }
}
