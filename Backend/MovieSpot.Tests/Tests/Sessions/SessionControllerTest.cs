using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieSpot.Tests.Controllers.Sessions
{
    public class SessionControllerTest
    {
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly SessionController _controller;

        public SessionControllerTest()
        {
            _sessionServiceMock = new Mock<ISessionService>();
            _controller = new SessionController(_sessionServiceMock.Object);
        }

        #region GetById

        [Fact]
        public void GetById_ReturnsOk_WhenSessionExists()
        {
            var session = new Session
            {
                Id = 1,
                MovieId = 10,
                Movie = new Movie { Title = "Inception" },
                CinemaHallId = 2,
                CreatedBy = 99,
                CreatedByUser = new User { Name = "Admin" },
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(2),
                Price = 8.5m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _sessionServiceMock.Setup(s => s.GetSessionById(1)).Returns(session);

            var result = _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<SessionDTO.SessionResponseDto>(ok.Value);
            Assert.Equal(session.Id, dto.Id);
            Assert.Equal("Inception", dto.MovieTitle);
        }

        [Fact]
        public void GetById_ReturnsOk_WithUnknownFields_WhenRelatedEntitiesAreNull()
        {
            var session = new Session
            {
                Id = 1,
                MovieId = 10,
                Movie = null,
                CinemaHallId = 2,
                CinemaHall = null,
                CreatedBy = 99,
                CreatedByUser = null,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(2),
                Price = 8.5m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _sessionServiceMock.Setup(s => s.GetSessionById(1)).Returns(session);

            var result = _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<SessionDTO.SessionResponseDto>(ok.Value);

            Assert.Equal("Unknown", dto.MovieTitle);
            Assert.Equal("Unknown", dto.CinemaHallName);
            Assert.Equal("Unknown", dto.CreatedByName);
        }

        [Fact]
        public void GetById_ReturnsBadRequest_OnArgumentOutOfRangeException()
        {
            _sessionServiceMock
                .Setup(s => s.GetSessionById(1))
                .Throws(new ArgumentOutOfRangeException("Invalid ID"));

            var result = _controller.GetById(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid ID", badRequest.Value!.ToString());
        }

        [Fact]
        public void GetById_ReturnsNotFound_OnKeyNotFoundException()
        {
            _sessionServiceMock
                .Setup(s => s.GetSessionById(1))
                .Throws(new KeyNotFoundException("Session not found"));

            var result = _controller.GetById(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Session not found", notFound.Value);
        }

        #endregion

        #region GetAll

        [Fact]
        public void GetAll_ReturnsOk_WhenSessionsExist()
        {
            var sessions = new List<Session>
            {
                new Session { Id = 1, MovieId = 1, Movie = new Movie { Title = "Movie 1" } },
                new Session { Id = 2, MovieId = 2, Movie = new Movie { Title = "Movie 2" } }
            };
            _sessionServiceMock.Setup(s => s.GetAllSessions()).Returns(sessions);

            var result = _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<SessionDTO.SessionResponseDto>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public void GetAll_ReturnsOk_WithUnknownFields_WhenRelatedEntitiesAreNull()
        {
            var sessions = new List<Session>
            {
                new Session
                {
                    Id = 1,
                    MovieId = 10,
                    Movie = null,
                    CinemaHallId = 2,
                    CinemaHall = null,
                    CreatedBy = 99,
                    CreatedByUser = null,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(2),
                    Price = 8.5m,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
            _sessionServiceMock.Setup(s => s.GetAllSessions()).Returns(sessions);

            var result = _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<SessionDTO.SessionResponseDto>>(ok.Value);
            var dto = list.First();

            Assert.Equal("Unknown", dto.MovieTitle);
            Assert.Equal("Unknown", dto.CinemaHallName);
            Assert.Equal("Unknown", dto.CreatedByName);
        }

        [Fact]
        public void GetAll_ReturnsNotFound_OnInvalidOperationException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAllSessions())
                .Throws(new InvalidOperationException("No sessions found"));

            var result = _controller.GetAll();

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No sessions found", notFound.Value);
        }

        #endregion

        #region Create

        [Fact]
        public void Create_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = _controller.Create(null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Session cannot be null.", bad.Value);
        }

        [Fact]
        public void Create_ReturnsOk_WhenSuccessful()
        {
            var dto = new SessionDTO.SessionCreateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                CreatedBy = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };
            var session = new Session
            {
                Id = 5,
                MovieId = dto.MovieId,
                CinemaHallId = dto.CinemaHallId,
                CreatedBy = dto.CreatedBy,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Price = dto.Price,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _sessionServiceMock.Setup(s => s.CreateSession(It.IsAny<Session>())).Returns(session);

            var result = _controller.Create(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SessionDTO.SessionResponseDto>(ok.Value);
            Assert.Equal(session.Id, response.Id);
        }

        [Fact]
        public void Create_ReturnsBadRequest_OnArgumentException()
        {
            _sessionServiceMock
                .Setup(s => s.CreateSession(It.IsAny<Session>()))
                .Throws(new ArgumentException("Error message"));

            var dto = new SessionDTO.SessionCreateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                CreatedBy = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Create(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error message", bad.Value);
        }

        [Fact]
        public void Create_ReturnsBadRequest_OnInvalidOperationException()
        {
            _sessionServiceMock
                .Setup(s => s.CreateSession(It.IsAny<Session>()))
                .Throws(new InvalidOperationException("Error message"));

            var dto = new SessionDTO.SessionCreateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                CreatedBy = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Create(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error message", bad.Value);
        }

        [Fact]
        public void Create_ReturnsBadRequest_OnDbUpdateException()
        {
            _sessionServiceMock
                .Setup(s => s.CreateSession(It.IsAny<Session>()))
                .Throws(new DbUpdateException("Error message"));

            var dto = new SessionDTO.SessionCreateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                CreatedBy = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Create(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error message", bad.Value);
        }

        [Fact]
        public void Create_ReturnsNotFound_OnKeyNotFoundException()
        {
            _sessionServiceMock
                .Setup(s => s.CreateSession(It.IsAny<Session>()))
                .Throws(new KeyNotFoundException("Not found"));

            var dto = new SessionDTO.SessionCreateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                CreatedBy = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Create(dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not found", notFound.Value);
        }

        #endregion

        #region Update

        [Fact]
        public void Update_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = _controller.Update(1, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Session cannot be null.", bad.Value);
        }

        [Fact]
        public void Update_ReturnsOk_WhenSuccessful()
        {
            var dto = new SessionDTO.SessionUpdateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };
            var session = new Session
            {
                Id = 1,
                MovieId = dto.MovieId,
                CinemaHallId = dto.CinemaHallId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Price = dto.Price,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _sessionServiceMock.Setup(s => s.UpdateSession(1, It.IsAny<Session>())).Returns(session);

            var result = _controller.Update(1, dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SessionDTO.SessionResponseDto>(ok.Value);
            Assert.Equal(session.Id, response.Id);
        }

        [Fact]
        public void Update_ReturnsBadRequest_OnArgumentOutOfRangeException()
        {
            _sessionServiceMock
                .Setup(s => s.UpdateSession(It.IsAny<int>(), It.IsAny<Session>()))
                .Throws(new ArgumentOutOfRangeException("Update error"));

            var dto = new SessionDTO.SessionUpdateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Update(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Update error", bad.Value!.ToString());
        }

        [Fact]
        public void Update_ReturnsBadRequest_OnArgumentNullException()
        {
            _sessionServiceMock
                .Setup(s => s.UpdateSession(It.IsAny<int>(), It.IsAny<Session>()))
                .Throws(new ArgumentNullException("Update error"));

            var dto = new SessionDTO.SessionUpdateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Update(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Update error", bad.Value!.ToString());
        }

        [Fact]
        public void Update_ReturnsBadRequest_OnInvalidOperationException()
        {
            _sessionServiceMock
                .Setup(s => s.UpdateSession(It.IsAny<int>(), It.IsAny<Session>()))
                .Throws(new InvalidOperationException("Update error"));

            var dto = new SessionDTO.SessionUpdateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Update(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update error", bad.Value);
        }

        [Fact]
        public void Update_ReturnsBadRequest_OnDbUpdateException()
        {
            _sessionServiceMock
                .Setup(s => s.UpdateSession(It.IsAny<int>(), It.IsAny<Session>()))
                .Throws(new DbUpdateException("Update error"));

            var dto = new SessionDTO.SessionUpdateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Update(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update error", bad.Value);
        }

        [Fact]
        public void Update_ReturnsNotFound_OnKeyNotFoundException()
        {
            _sessionServiceMock
                .Setup(s => s.UpdateSession(It.IsAny<int>(), It.IsAny<Session>()))
                .Throws(new KeyNotFoundException("Not found"));

            var dto = new SessionDTO.SessionUpdateDto
            {
                MovieId = 1,
                CinemaHallId = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 10
            };

            var result = _controller.Update(1, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not found", notFound.Value);
        }

        #endregion

        #region Delete

        [Fact]
        public void Delete_ReturnsOk_WhenSuccessful()
        {
            var session = new Session { Id = 1, MovieId = 1 };
            _sessionServiceMock.Setup(s => s.DeleteSession(1)).Returns(session);

            var result = _controller.Delete(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SessionDTO.SessionResponseDto>(ok.Value);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        public void Delete_ReturnsBadRequest_OnArgumentOutOfRangeException()
        {
            _sessionServiceMock
                .Setup(s => s.DeleteSession(1))
                .Throws(new ArgumentOutOfRangeException("Delete error"));

            var result = _controller.Delete(1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Delete error", bad.Value!.ToString());
        }

        [Fact]
        public void Delete_ReturnsBadRequest_OnDbUpdateException()
        {
            _sessionServiceMock
                .Setup(s => s.DeleteSession(1))
                .Throws(new DbUpdateException("Delete error"));

            var result = _controller.Delete(1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Delete error", bad.Value);
        }

        [Fact]
        public void Delete_ReturnsNotFound_OnKeyNotFoundException()
        {
            _sessionServiceMock
                .Setup(s => s.DeleteSession(1))
                .Throws(new KeyNotFoundException("Not found"));

            var result = _controller.Delete(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not found", notFound.Value);
        }

        #endregion

        #region GetAvailableTimes

        [Fact]
        public void GetAvailableTimes_ReturnsOk_WhenSuccessful()
        {
            var times = new List<TimeSpan> { new(10, 0, 0), new(14, 30, 0) };
            _sessionServiceMock.Setup(s => s.GetAvailableTimes(1, It.IsAny<DateTime>(), 120)).Returns(times);

            var result = _controller.GetAvailableTimes(1, DateTime.Today, 120);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<TimeSpan>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public void GetAvailableTimes_ReturnsBadRequest_OnArgumentOutOfRangeException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableTimes(1, It.IsAny<DateTime>(), 120))
                .Throws(new ArgumentOutOfRangeException("Invalid input"));

            var result = _controller.GetAvailableTimes(1, DateTime.Today, 120);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid input", bad.Value!.ToString());
        }

        [Fact]
        public void GetAvailableTimes_ReturnsNotFound_OnInvalidOperationException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableTimes(1, It.IsAny<DateTime>(), 120))
                .Throws(new InvalidOperationException("No available times"));

            var result = _controller.GetAvailableTimes(1, DateTime.Today, 120);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No available times", notFound.Value);
        }

        [Fact]
        public void GetAvailableTimes_ReturnsBadRequest_OnGenericException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableTimes(1, It.IsAny<DateTime>(), 120))
                .Throws(new Exception("Unknown error"));

            var result = _controller.GetAvailableTimes(1, DateTime.Today, 120);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unexpected error while retrieving available times", bad.Value!.ToString());
        }

        #endregion

        #region GetAvailableSeats

        [Fact]
        public void GetAvailableSeats_ReturnsOk_WhenSuccessful()
        {
            var seats = new List<Seat>
            {
                new Seat { Id = 1, SeatNumber = "A1", SeatType = "Normal" },
                new Seat { Id = 2, SeatNumber = "A2", SeatType = "VIP" }
            };
            _sessionServiceMock.Setup(s => s.GetAvailableSeats(1)).Returns(seats);

            var result = _controller.GetAvailableSeats(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<SessionDTO.AvailableSeatDto>>(ok.Value);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, s => s.SeatNumber == "A1");
        }

        [Fact]
        public void GetAvailableSeats_ReturnsBadRequest_OnArgumentOutOfRangeException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableSeats(1))
                .Throws(new ArgumentOutOfRangeException("Invalid ID"));

            var result = _controller.GetAvailableSeats(1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid ID", bad.Value!.ToString());
        }

        [Fact]
        public void GetAvailableSeats_ReturnsNotFound_OnKeyNotFoundException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableSeats(1))
                .Throws(new KeyNotFoundException("Session not found"));

            var result = _controller.GetAvailableSeats(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Session not found", notFound.Value);
        }

        [Fact]
        public void GetAvailableSeats_ReturnsNotFound_OnInvalidOperationException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableSeats(1))
                .Throws(new InvalidOperationException("No seats available"));

            var result = _controller.GetAvailableSeats(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No seats available", notFound.Value);
        }

        [Fact]
        public void GetAvailableSeats_ReturnsBadRequest_OnGenericException()
        {
            _sessionServiceMock
                .Setup(s => s.GetAvailableSeats(1))
                .Throws(new Exception("Unexpected error"));

            var result = _controller.GetAvailableSeats(1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unexpected error while retrieving available seats", bad.Value!.ToString());
        }

        #endregion
    }
}
