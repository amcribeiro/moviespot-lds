using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Notifications;
using MovieSpot.Services.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MovieSpot.Tests.Services.Sessions
{
    public class SessionServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IFcmNotificationService> _fcmMock;
        private readonly SessionService _service;

        public SessionServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _fcmMock = new Mock<IFcmNotificationService>();
            _service = new SessionService(_context, _fcmMock.Object);

            _context.Movie.Add(new Movie
            {
                Id = 1,
                Title = "Matrix",
                Description = "Um clássico da ficção científica.",
                Duration = 120,
                ReleaseDate = DateTime.Today,
                Language = "Inglês",
                Country = "EUA"
            });

            _context.CinemaHall.Add(new CinemaHall { Id = 1, Name = "Sala 1" });
            _context.User.Add(new User { Id = 1, Name = "Admin", Email = "admin@x.com" });
            _context.SaveChanges();
        }

        #region CreateSession

        [Fact]
        public void CreateSession_Should_Add_New_Session()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(12),
                Price = 5
            };

            var result = _service.CreateSession(session);

            Assert.NotNull(result);
            Assert.Single(_context.Session);
        }

        [Fact]
        public void CreateSession_Should_Throw_When_Session_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.CreateSession(null));
        }

        [Fact]
        public void CreateSession_Should_Throw_When_StartDate_After_EndDate()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(15),
                EndDate = DateTime.Today.AddHours(14),
                Price = 5
            };

            Assert.Throws<ArgumentException>(() => _service.CreateSession(session));
        }

        [Fact]
        public void CreateSession_Should_Throw_When_Price_Negative()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(12),
                Price = -10
            };

            Assert.Throws<ArgumentException>(() => _service.CreateSession(session));
        }

        [Fact]
        public void CreateSession_Should_Throw_When_Movie_Not_Exists()
        {
            var session = new Session
            {
                MovieId = 99,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(12),
                Price = 5
            };

            Assert.Throws<KeyNotFoundException>(() => _service.CreateSession(session));
        }

        [Fact]
        public void CreateSession_Should_Throw_When_CinemaHall_Not_Exists()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 999,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(12),
                Price = 5
            };

            Assert.Throws<KeyNotFoundException>(() => _service.CreateSession(session));
        }

        [Fact]
        public void CreateSession_Should_Throw_When_User_Not_Exists()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 999,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(12),
                Price = 5
            };

            Assert.Throws<KeyNotFoundException>(() => _service.CreateSession(session));
        }

        [Fact]
        public void CreateSession_Should_Throw_When_Conflicts_With_Existing()
        {
            _context.Session.Add(new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(12),
                Price = 5
            });
            _context.SaveChanges();

            var conflictSession = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(11),
                EndDate = DateTime.Today.AddHours(13),
                Price = 5
            };

            Assert.Throws<InvalidOperationException>(() => _service.CreateSession(conflictSession));
        }

        #endregion

        #region DeleteSession

        [Fact]
        public void DeleteSession_Should_Remove_Session()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            };
            _context.Session.Add(session);
            _context.SaveChanges();

            var deleted = _service.DeleteSession(session.Id);

            Assert.Equal(session.Id, deleted.Id);
            Assert.Empty(_context.Session);
        }

        [Fact]
        public void DeleteSession_Should_Throw_When_Id_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.DeleteSession(0));
        }

        [Fact]
        public void DeleteSession_Should_Throw_When_Not_Found()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.DeleteSession(999));
        }

        #endregion

        #region GetAllSessions

        [Fact]
        public void GetAllSessions_Should_Return_List()
        {
            _context.Session.Add(new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            });
            _context.SaveChanges();

            var sessions = _service.GetAllSessions();

            Assert.Single(sessions);
        }

        [Fact]
        public void GetAllSessions_Should_Throw_When_Empty()
        {
            Assert.Throws<InvalidOperationException>(() => _service.GetAllSessions());
        }

        #endregion

        #region GetSessionById

        [Fact]
        public void GetSessionById_Should_Return_Session()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            };
            _context.Session.Add(session);
            _context.SaveChanges();

            var found = _service.GetSessionById(session.Id);

            Assert.Equal(session.Id, found.Id);
        }

        [Fact]
        public void GetSessionById_Should_Throw_When_Id_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetSessionById(0));
        }

        [Fact]
        public void GetSessionById_Should_Throw_When_Not_Found()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetSessionById(999));
        }

        #endregion

        #region UpdateSession

        [Fact]
        public void UpdateSession_Should_Modify_Session()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            };
            _context.Session.Add(session);
            _context.SaveChanges();

            var updated = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                StartDate = DateTime.Now.AddHours(13),
                EndDate = DateTime.Now.AddHours(15),
                Price = 10
            };

            var result = _service.UpdateSession(session.Id, updated);

            Assert.Equal(10, result.Price);
            Assert.Equal(updated.StartDate.Hour, result.StartDate.Hour);
        }

        [Fact]
        public void UpdateSession_Should_Throw_When_Id_Invalid()
        {
            var updated = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 5
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateSession(0, updated));
        }

        [Fact]
        public void UpdateSession_Should_Throw_When_Session_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.UpdateSession(1, null));
        }

        [Fact]
        public void UpdateSession_Should_Throw_When_Not_Found()
        {
            var updated = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Price = 5
            };

            Assert.Throws<KeyNotFoundException>(() => _service.UpdateSession(999, updated));
        }

        [Fact]
        public void UpdateSession_Should_Throw_When_Conflicts_With_Other_Session()
        {
            var s1 = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            };
            var s2 = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(13),
                EndDate = DateTime.Now.AddHours(15),
                Price = 5
            };
            _context.Session.AddRange(s1, s2);
            _context.SaveChanges();

            var updated = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                StartDate = s2.StartDate.AddMinutes(-30),
                EndDate = s2.EndDate,
                Price = 8
            };

            Assert.Throws<InvalidOperationException>(() => _service.UpdateSession(s1.Id, updated));
        }

        #endregion

        #region GetAvailableTimes

        [Fact]
        public void GetAvailableTimes_Should_Return_Times()
        {
            _context.Session.Add(new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(12),
                EndDate = DateTime.Today.AddHours(14),
                Price = 5
            });
            _context.SaveChanges();

            var times = _service.GetAvailableTimes(1, DateTime.Today, 60);

            Assert.NotEmpty(times);
        }

        [Fact]
        public void GetAvailableTimes_Should_Throw_When_CinemaHallId_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetAvailableTimes(0, DateTime.Today, 60));
        }

        [Fact]
        public void GetAvailableTimes_Should_Throw_When_Runtime_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetAvailableTimes(1, DateTime.Today, 0));
        }

        [Fact]
        public void GetAvailableTimes_Should_Throw_When_No_Available_Slots()
        {
            _context.Session.Add(new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Today.AddHours(10),
                EndDate = DateTime.Today.AddHours(23),
                Price = 5
            });
            _context.SaveChanges();

            Assert.Throws<InvalidOperationException>(() => _service.GetAvailableTimes(1, DateTime.Today, 60));
        }

        #endregion

        #region GetAvailableSeats

        [Fact]
        public void GetAvailableSeats_Should_Return_Available_Seats()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            };
            _context.Session.Add(session);

            var seats = new List<Seat>
    {
        new Seat { Id = 1, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Normal" },
        new Seat { Id = 2, CinemaHallId = 1, SeatNumber = "A2", SeatType = "Normal" },
        new Seat { Id = 3, CinemaHallId = 1, SeatNumber = "A3", SeatType = "Normal" }
    };
            _context.Seat.AddRange(seats);

            var booking = new Booking
            {
                Id = 1,
                UserId = 1,
                SessionId = session.Id,
                BookingDate = DateTime.UtcNow,
                TotalAmount = 10,
                Status = true
            };
            _context.Booking.Add(booking);

            var bookedSeat = new BookingSeat
            {
                BookingId = 1,
                SeatId = 2,
                SeatPrice = 5,
                CreatedAt = DateTime.UtcNow
            };
            _context.BookingSeat.Add(bookedSeat);

            _context.SaveChanges();

            var result = _service.GetAvailableSeats(session.Id).ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, s => s.Id == 2);
        }

        [Fact]
        public void GetAvailableSeats_Should_Throw_When_SessionId_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetAvailableSeats(0));
        }

        [Fact]
        public void GetAvailableSeats_Should_Throw_When_Session_Not_Found()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetAvailableSeats(999));
        }

        [Fact]
        public void GetAvailableSeats_Should_Throw_When_No_Seats_Available()
        {
            var session = new Session
            {
                MovieId = 1,
                CinemaHallId = 1,
                CreatedBy = 1,
                StartDate = DateTime.Now.AddHours(10),
                EndDate = DateTime.Now.AddHours(12),
                Price = 5
            };
            _context.Session.Add(session);

            var seats = new List<Seat>
    {
        new Seat { Id = 1, CinemaHallId = 1, SeatNumber = "A1", SeatType = "Normal" }
    };
            _context.Seat.AddRange(seats);

            var booking = new Booking
            {
                Id = 1,
                UserId = 1,
                SessionId = session.Id,
                BookingDate = DateTime.UtcNow,
                TotalAmount = 5,
                Status = true
            };
            _context.Booking.Add(booking);

            _context.BookingSeat.Add(new BookingSeat
            {
                BookingId = 1,
                SeatId = 1,
                SeatPrice = 5,
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            Assert.Throws<InvalidOperationException>(() => _service.GetAvailableSeats(session.Id));
        }

        #endregion

    }
}
