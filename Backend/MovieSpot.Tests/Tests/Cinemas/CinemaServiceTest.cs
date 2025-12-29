using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Cinemas;
using Xunit;

namespace MovieSpot.Tests.Services.Cinemas
{
    public class CinemaServiceTest
    {
        private static ApplicationDbContext NewCtx()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private class FaultyCtx : ApplicationDbContext
        {
            public bool ThrowOnSave { get; set; }
            public FaultyCtx(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            public override int SaveChanges()
            {
                if (ThrowOnSave) throw new DbUpdateException("Simulated SaveChanges failure");
                return base.SaveChanges();
            }
        }

        private static FaultyCtx NewFaultyCtx(bool throwOnSave)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FaultyCtx(options) { ThrowOnSave = throwOnSave };
        }

        private static Cinema MakeCinema(int id = 1) => new()
        {
            Id = id,
            Name = "Cinema Alfa",
            Street = "Rua A",
            City = "Porto",
            State = "PT",
            ZipCode = "4000-000",
            Country = "PT",
            Latitude = 41.15M,
            Longitude = -8.61M,
            CreatedAt = DateTime.UtcNow
        };

        [Fact]
        public void GetAllCinemas_Empty_Throws()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetAllCinemas());
            Assert.Contains("no cinemas registered", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllCinemas_Returns_WithHalls()
        {
            using var ctx = NewCtx();
            var c = MakeCinema(10);
            ctx.Cinema.Add(c);
            ctx.CinemaHall.Add(new CinemaHall { Id = 100, Name = "Sala 1", CinemaId = 10, CreatedAt = DateTime.UtcNow });
            ctx.SaveChanges();

            var sut = new CinemaService(ctx);
            var list = sut.GetAllCinemas();

            Assert.Single(list);
            Assert.Single(list[0].CinemaHalls);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void GetCinemaById_Invalid_Throws(int id)
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetCinemaById(id));
        }

        [Fact]
        public void GetCinemaById_NotFound_Throws()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            var ex = Assert.Throws<KeyNotFoundException>(() => sut.GetCinemaById(99));
            Assert.Contains("99", ex.Message);
        }

        [Fact]
        public void GetCinemaById_Found_Returns()
        {
            using var ctx = NewCtx();
            var c = MakeCinema(2);
            ctx.Cinema.Add(c);
            ctx.CinemaHall.Add(new CinemaHall { Id = 201, Name = "Sala A", CinemaId = 2, CreatedAt = DateTime.UtcNow });
            ctx.SaveChanges();

            var sut = new CinemaService(ctx);
            var got = sut.GetCinemaById(2);

            Assert.Equal("Cinema Alfa", got.Name);
            Assert.Single(got.CinemaHalls);
        }

        [Fact]
        public void AddCinema_Null_Throws()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            Assert.Throws<ArgumentNullException>(() => sut.AddCinema(null!));
        }

        [Fact]
        public void AddCinema_SaveFails_ThrowsWrappedDbUpdateException()
        {
            using var ctx = NewFaultyCtx(throwOnSave: true);
            var sut = new CinemaService(ctx);
            var c = MakeCinema(5);

            var ex = Assert.Throws<DbUpdateException>(() => sut.AddCinema(c));
            Assert.Contains("saving the new cinema", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddCinema_Success_Persists()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);
            var c = MakeCinema(3);

            sut.AddCinema(c);

            var stored = ctx.Cinema.Single();
            Assert.Equal(3, stored.Id);
            Assert.Equal("Cinema Alfa", stored.Name);
        }

        [Fact]
        public void UpdateCinema_Null_Throws()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            Assert.Throws<ArgumentNullException>(() => sut.UpdateCinema(null!));
        }

        [Fact]
        public void UpdateCinema_NotFound_Throws()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            var ex = Assert.Throws<KeyNotFoundException>(() => sut.UpdateCinema(MakeCinema(77)));
            Assert.Contains("77", ex.Message);
        }

        [Fact]
        public void UpdateCinema_SaveFails_ThrowsWrappedDbUpdateException()
        {
            using var ctx = NewFaultyCtx(throwOnSave: false);
            ctx.Cinema.Add(MakeCinema(7));
            ctx.SaveChanges();
            ctx.ThrowOnSave = true;

            var sut = new CinemaService(ctx);
            var updated = MakeCinema(7);
            updated.Name = "Novo Nome";

            var ex = Assert.Throws<DbUpdateException>(() => sut.UpdateCinema(updated));
            Assert.Contains("updating the cinema", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateCinema_Success_UpdatesFields()
        {
            using var ctx = NewCtx();
            var exi = MakeCinema(8);
            exi.Name = "Antigo";
            ctx.Cinema.Add(exi);
            ctx.SaveChanges();

            var sut = new CinemaService(ctx);
            var upd = MakeCinema(8);
            upd.Name = "Novo";
            upd.City = "Lisboa";
            upd.Latitude = 38.72M;
            upd.Longitude = -9.13M;

            sut.UpdateCinema(upd);

            var stored = ctx.Cinema.Single(c => c.Id == 8);
            Assert.Equal("Novo", stored.Name);
            Assert.Equal("Lisboa", stored.City);
            Assert.Equal(38.72M, stored.Latitude);
            Assert.Equal(-9.13M, stored.Longitude);
            Assert.NotEqual(default, stored.UpdatedAt);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RemoveCinema_Invalid_Throws(int id)
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.RemoveCinema(id));
        }

        [Fact]
        public void RemoveCinema_NotFound_Throws()
        {
            using var ctx = NewCtx();
            var sut = new CinemaService(ctx);

            var ex = Assert.Throws<KeyNotFoundException>(() => sut.RemoveCinema(42));
            Assert.Contains("42", ex.Message);
        }

        [Fact]
        public void RemoveCinema_SaveFails_ThrowsWrappedDbUpdateException()
        {
            using var ctx = NewFaultyCtx(throwOnSave: false);
            ctx.Cinema.Add(MakeCinema(12));
            ctx.SaveChanges();
            ctx.ThrowOnSave = true;

            var sut = new CinemaService(ctx);

            var ex = Assert.Throws<DbUpdateException>(() => sut.RemoveCinema(12));
            Assert.Contains("removing the cinema", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RemoveCinema_Success_Removes()
        {
            using var ctx = NewCtx();
            ctx.Cinema.Add(MakeCinema(13));
            ctx.SaveChanges();

            var sut = new CinemaService(ctx);
            sut.RemoveCinema(13);

            Assert.Empty(ctx.Cinema.ToList());
        }
    }
}
