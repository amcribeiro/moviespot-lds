using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.CinemaHalls;
using Xunit;

namespace MovieSpot.Tests.Services.CinemaHalls
{
    public class FakeDbContext : ApplicationDbContext
    {
        private readonly bool _failOnSave;

        public FakeDbContext(DbContextOptions<ApplicationDbContext> options, bool failOnSave = false)
            : base(options)
        {
            _failOnSave = failOnSave;
        }

        public override int SaveChanges()
        {
            if (_failOnSave)
                throw new DbUpdateException("Simulated DB failure");
            return base.SaveChanges();
        }
    }

    public class CinemaHallServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly CinemaHallService _service;

        public CinemaHallServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new CinemaHallService(_context);
        }

        private void SeedData()
        {
            var cinema = new Cinema
            {
                Id = 1,
                Name = "CineMax Porto",
                Street = "Rua Central 12",
                City = "Porto",
                Country = "Portugal",
                Latitude = 41.15M,
                Longitude = -8.61M
            };

            var hall1 = new CinemaHall
            {
                Id = 10,
                Name = "Sala 1",
                CinemaId = 1
            };

            var hall2 = new CinemaHall
            {
                Id = 11,
                Name = "Sala 2",
                CinemaId = 1
            };

            _context.Cinema.Add(cinema);
            _context.CinemaHall.AddRange(hall1, hall2);
            _context.SaveChanges();
        }

        [Fact]
        public void GetAllCinemaHalls_WhenExist_ReturnsList()
        {
            SeedData();

            var result = _service.GetAllCinemaHalls().ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, h => h.Name == "Sala 1");
        }

        [Fact]
        public void GetAllCinemaHalls_WhenEmpty_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _service.GetAllCinemaHalls());
        }

        [Fact]
        public void GetCinemaHallById_ValidId_ReturnsEntity()
        {
            SeedData();

            var hall = _service.GetCinemaHallById(10);

            Assert.NotNull(hall);
            Assert.Equal("Sala 1", hall.Name);
        }

        [Fact]
        public void GetCinemaHallById_IdZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetCinemaHallById(0));
        }

        [Fact]
        public void GetCinemaHallById_NotFound_ThrowsKeyNotFoundException()
        {
            SeedData();
            Assert.Throws<KeyNotFoundException>(() => _service.GetCinemaHallById(999));
        }

        [Fact]
        public void GetCinemaHallsByCinemaId_Valid_ReturnsList()
        {
            SeedData();

            var halls = _service.GetCinemaHallsByCinemaId(1).ToList();

            Assert.NotNull(halls);
            Assert.Equal(2, halls.Count);
        }

        [Fact]
        public void GetCinemaHallsByCinemaId_InvalidId_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetCinemaHallsByCinemaId(0));
        }

        [Fact]
        public void GetCinemaHallsByCinemaId_NoHalls_ThrowsKeyNotFoundException()
        {
            var cinema = new Cinema
            {
                Id = 2,
                Name = "Cine Lisboa",
                Street = "Av. Liberdade 50",
                City = "Lisboa",
                Country = "Portugal",
                Latitude = 38.72M,
                Longitude = -9.13M
            };
            _context.Cinema.Add(cinema);
            _context.SaveChanges();

            Assert.Throws<KeyNotFoundException>(() => _service.GetCinemaHallsByCinemaId(2));
        }

        [Fact]
        public void AddCinemaHall_ValidData_SavesEntity()
        {
            var hall = new CinemaHall
            {
                Name = "Nova Sala",
                CinemaId = 1
            };

            var added = _service.AddCinemaHall(hall);

            Assert.NotNull(added);
            Assert.Equal("Nova Sala", added.Name);
            Assert.Single(_context.CinemaHall.ToList());
        }

        [Fact]
        public void AddCinemaHall_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddCinemaHall(null));
        }

        [Fact]
        public void AddCinemaHall_WhenSaveChangesFails_ThrowsDbUpdateException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FakeDbContext(options, failOnSave: true);
            var service = new CinemaHallService(failingContext);

            var hall = new CinemaHall { Name = "Erro Sala", CinemaId = 1 };

            var ex = Assert.Throws<DbUpdateException>(() => service.AddCinemaHall(hall));
            Assert.Contains("saving the new cinema hall", ex.Message);
        }


        [Fact]
        public void UpdateCinemaHall_Valid_UpdatesData()
        {
            SeedData();

            var updated = new CinemaHall
            {
                Id = 10,
                Name = "Sala Reformada",
                CinemaId = 1
            };

            var result = _service.UpdateCinemaHall(10, updated);

            Assert.Equal("Sala Reformada", result.Name);
        }

        [Fact]
        public void UpdateCinemaHall_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.UpdateCinemaHall(1, null));
        }

        [Fact]
        public void UpdateCinemaHall_NotFound_ThrowsKeyNotFoundException()
        {
            var hall = new CinemaHall { Id = 99, Name = "Fantasma", CinemaId = 1 };
            Assert.Throws<KeyNotFoundException>(() => _service.UpdateCinemaHall(99, hall));
        }

        [Fact]
        public void UpdateCinemaHall_IdZero_ThrowsArgumentOutOfRangeException()
        {
            var hall = new CinemaHall { Id = 1, Name = "Sala Teste", CinemaId = 1 };
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateCinemaHall(0, hall));
        }

        [Fact]
        public void UpdateCinemaHall_WhenSaveChangesFails_ThrowsDbUpdateException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new FakeDbContext(options);
            var hall = new CinemaHall { Id = 1, Name = "Sala 1", CinemaId = 1 };
            context.CinemaHall.Add(hall);
            context.SaveChanges();

            var failingContext = new FakeDbContext(options, failOnSave: true);
            var service = new CinemaHallService(failingContext);

            var updated = new CinemaHall { Name = "Erro", CinemaId = 1 };

            var ex = Assert.Throws<DbUpdateException>(() => service.UpdateCinemaHall(1, updated));
            Assert.Contains("updating the cinema hall", ex.Message);

        }

        [Fact]
        public void RemoveCinemaHall_ValidId_Removes()
        {
            SeedData();

            _service.RemoveCinemaHall(10);

            Assert.False(_context.CinemaHall.Any(h => h.Id == 10));
        }

        [Fact]
        public void RemoveCinemaHall_IdZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.RemoveCinemaHall(0));
        }

        [Fact]
        public void RemoveCinemaHall_NotFound_ThrowsKeyNotFoundException()
        {
            SeedData();
            Assert.Throws<KeyNotFoundException>(() => _service.RemoveCinemaHall(999));
        }

        [Fact]
        public void RemoveCinemaHall_WhenSaveChangesFails_ThrowsDbUpdateException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new FakeDbContext(options);
            var hall = new CinemaHall { Id = 1, Name = "Sala 1", CinemaId = 1 };
            context.CinemaHall.Add(hall);
            context.SaveChanges();

            var failingContext = new FakeDbContext(options, failOnSave: true);
            var service = new CinemaHallService(failingContext);

            var ex = Assert.Throws<DbUpdateException>(() => service.RemoveCinemaHall(1));
            Assert.Contains("removing the cinema hall", ex.Message);
        }
    }
}
