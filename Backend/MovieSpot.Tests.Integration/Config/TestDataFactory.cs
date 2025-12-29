using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Handlers;

namespace MovieSpot.Tests.Integration.Config
{
    public class TestDataFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPasswordService _passwordService;

        public TestDataFactory(HttpClient client, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            using var scope = _serviceProvider.CreateScope();
            _passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        }
        /// <summary>
        /// Limpa todas as tabelas principais usadas nos testes.
        /// </summary>
        public async Task ClearDatabaseAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Booking.RemoveRange(db.Booking);
            db.Session.RemoveRange(db.Session);
            db.CinemaHall.RemoveRange(db.CinemaHall);
            db.Cinema.RemoveRange(db.Cinema);
            db.User.RemoveRange(db.User);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Cria e guarda um utilizador de teste diretamente na base de dados.
        /// </summary>
        public async Task<User> CreateTestUserAsync(string role = "User")
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = new User
            {
                Name = $"{role}_User_{Guid.NewGuid()}",
                Email = $"{role.ToLower()}_{Guid.NewGuid()}@example.com",
                Password = _passwordService.HashPassword("123456"),
                Phone = "912345678",
                Role = role,
                AccountStatus = "Active"
            };

            db.User.Add(user);
            await db.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Cria e guarda um cinema de teste diretamente na base de dados.
        /// </summary>
        public async Task<Cinema> CreateTestCinemaAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cinema = new Cinema
            {
                Name = $"Cinema_{Guid.NewGuid()}",
                Street = "Rua Teste",
                City = "Porto",
                Country = "Portugal",
                Latitude = 41.15m,
                Longitude = -8.61m
            };

            db.Cinema.Add(cinema);
            await db.SaveChangesAsync();

            return cinema;
        }

        /// <summary>
        /// Cria e guarda uma sala de cinema de teste diretamente na base de dados.
        /// </summary>
        public async Task<CinemaHall> CreateTestCinemaHallAsync()
        {
            var cinema = await CreateTestCinemaAsync();

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var hall = new CinemaHall
            {
                Name = $"Sala {Guid.NewGuid()}",
                CinemaId = cinema.Id
            };

            db.CinemaHall.Add(hall);
            await db.SaveChangesAsync();

            return hall;
        }

        /// <summary>
        /// Cria e guarda um assento de teste diretamente na base de dados.
        /// </summary>
        public async Task<Seat> CreateTestSeatAsync()
        {
            var hall = await CreateTestCinemaHallAsync();

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var seat = new Seat
            {
                CinemaHallId = hall.Id,
                SeatNumber = "A1",
                SeatType = "Normal"
            };

            db.Seat.Add(seat);
            await db.SaveChangesAsync();

            return seat;
        }

        /// <summary>
        /// Cria e guarda uma sessão de teste diretamente na base de dados.
        /// </summary>
        public async Task<Session> CreateTestSessionAsync(int? createdById = null)
        {
            var movie = await CreateTestMovieAsync(); // ✅ cria um filme real
            var cinemaHall = await CreateTestCinemaHallAsync();
            var user = createdById == null ? await CreateTestUserAsync("Staff") : null;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var session = new Session
            {
                MovieId = movie.Id, // ✅ agora é um filme real
                CinemaHallId = cinemaHall.Id,
                CreatedBy = createdById ?? user!.Id,
                StartDate = DateTime.UtcNow.AddHours(2),
                EndDate = DateTime.UtcNow.AddHours(4),
                Price = 15,
                PromotionValue = 0
            };

            db.Session.Add(session);
            await db.SaveChangesAsync();

            return session;
        }

        /// <summary>
        /// Cria e guarda uma reserva de teste diretamente na base de dados.
        /// </summary>
        public async Task<Booking> CreateTestBookingAsync()
        {
            var user = await CreateTestUserAsync();
            var session = await CreateTestSessionAsync(user.Id);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var booking = new Booking
            {
                UserId = user.Id,
                SessionId = session.Id,
                TotalAmount = 25,
                BookingDate = DateTime.UtcNow,
                Status = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Booking.Add(booking);
            await db.SaveChangesAsync();

            return booking;
        }

        /// <summary>
        /// Cria e guarda um género de filme diretamente na base de dados.
        /// </summary>
        public async Task<Genre> CreateTestGenreAsync(string name = "Action")
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var genre = new Genre
            {
                Name = name
            };

            db.Genre.Add(genre);
            await db.SaveChangesAsync();

            return genre;
        }

        /// <summary>
        /// Cria e guarda um filme de teste diretamente na base de dados.
        /// </summary>
        public async Task<Movie> CreateTestMovieAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var movie = new Movie
            {
                Title = $"Movie_{Guid.NewGuid()}",
                Description = "A sample movie for testing.",
                Duration = 120,
                ReleaseDate = DateTime.UtcNow.AddYears(-1),
                Language = "English",
                Country = "USA",
                PosterPath = "/poster.jpg"
            };

            db.Movie.Add(movie);
            await db.SaveChangesAsync();

            return movie;
        }

        /// <summary>
        /// Cria e guarda uma review de teste diretamente na base de dados.
        /// </summary>
        public async Task<Review> CreateTestReviewAsync()
        {
            var booking = await CreateTestBookingAsync();

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var review = new Review
            {
                BookingId = booking.Id,
                Rating = 4,
                Comment = "Muito bom!",
                ReviewDate = DateTime.UtcNow
            };

            db.Review.Add(review);
            await db.SaveChangesAsync();

            return review;
        }

        /// <summary>
        /// Obtém o ID do utilizador associado a uma reserva.
        /// </summary>
        public async Task<int> GetUserIdByBookingIdAsync(int bookingId)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var userId = await db.Booking
                .Where(b => b.Id == bookingId)
                .Select(b => b.UserId)
                .FirstAsync();

            return userId;
        }

        /// <summary>
        /// Cria uma sessão completa com cinema, sala e pelo menos um assento disponível.
        /// </summary>
        public async Task<(Session session, Seat seat)> CreateTestSessionWithSeatAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cinema = new Cinema
            {
                Name = $"Cinema_{Guid.NewGuid()}",
                Street = "Rua Teste",
                City = "Lisboa",
                Country = "Portugal",
                Latitude = 38.72m,
                Longitude = -9.14m
            };
            db.Cinema.Add(cinema);
            await db.SaveChangesAsync();

            var hall = new CinemaHall
            {
                CinemaId = cinema.Id,
                Name = $"Sala_{Guid.NewGuid()}"
            };
            db.CinemaHall.Add(hall);
            await db.SaveChangesAsync();

            var movie = new Movie
            {
                Title = $"Movie_{Guid.NewGuid()}",
                Description = "Filme de teste para sessão com lugares.",
                Duration = 120,
                ReleaseDate = DateTime.UtcNow.AddYears(-1),
                Language = "Português",
                Country = "Portugal",
                PosterPath = "/poster.jpg"
            };
            db.Movie.Add(movie);
            await db.SaveChangesAsync();

            var staff = new User
            {
                Name = "Staff_Teste",
                Email = $"staff_{Guid.NewGuid()}@example.com",
                Password = "123456",
                Phone = "910000000",
                Role = "Staff",
                AccountStatus = "Active"
            };
            db.User.Add(staff);
            await db.SaveChangesAsync();

            var session = new Session
            {
                MovieId = movie.Id,
                CinemaHallId = hall.Id,
                CreatedBy = staff.Id,
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddHours(3),
                Price = 10,
                PromotionValue = 0
            };
            db.Session.Add(session);
            await db.SaveChangesAsync();

            var seat = new Seat
            {
                CinemaHallId = hall.Id,
                SeatNumber = "A1",
                SeatType = "Normal"
            };
            db.Seat.Add(seat);
            await db.SaveChangesAsync();

            return (session, seat);
        }
        public async Task<Payment> CreateTestPaymentAsync()
        {
            var booking = await CreateTestBookingAsync();

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var payment = new Payment
            {
                BookingId = booking.Id,
                PaymentMethod = "Stripe",
                PaymentStatus = "Pending",
                AmountPaid = booking.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Payment.Add(payment);
            await db.SaveChangesAsync();

            return payment;
        }
    }
}
