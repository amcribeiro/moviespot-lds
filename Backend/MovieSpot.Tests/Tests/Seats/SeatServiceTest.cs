using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Seats;
using Xunit;

namespace MovieSpot.Tests.Services.Seats
{
    public class SeatServiceTest
    {
        private static ApplicationDbContext NewCtx()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private static CinemaHall MakeHall(int id, int cinemaId = 1, string name = "Sala 1") => new()
        {
            Id = id,
            Name = name,
            CinemaId = cinemaId,
            CreatedAt = DateTime.UtcNow
        };

        private static Seat MakeSeat(int id, int hallId, string number = "A1", string type = "Standard") => new()
        {
            Id = id,
            CinemaHallId = hallId,
            SeatNumber = number,
            SeatType = type,
            CreatedAt = DateTime.UtcNow
        };

        private static void SeedBasic(ApplicationDbContext ctx)
        {
            ctx.Cinema.Add(new Cinema
            {
                Id = 1,
                Name = "CineX",
                Street = "S",
                City = "C",
                State = "ST",
                ZipCode = "0000-000",
                Country = "PT",
                Latitude = 0,
                Longitude = 0,
                CreatedAt = DateTime.UtcNow
            });
            ctx.CinemaHall.Add(MakeHall(10, 1));
            ctx.SaveChanges();
        }

        [Fact]
        public async Task GetAllSeatsAsync_ReturnsOrdered()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.AddRange(MakeSeat(1, 10, "B2"), MakeSeat(2, 10, "A1"), MakeSeat(3, 10, "C3"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var list = await sut.GetAllSeatsAsync();

            Assert.Equal(new[] { "A1", "B2", "C3" }, list.Select(s => s.SeatNumber).ToArray());
        }

        [Fact]
        public async Task GetSeatByIdAsync_ReturnsNull_WhenNotFound()
        {
            using var ctx = NewCtx();
            var sut = new SeatService(ctx);

            Assert.Null(await sut.GetSeatByIdAsync(999));
        }

        [Fact]
        public async Task GetSeatByIdAsync_ReturnsSeat_WhenFound()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.Add(MakeSeat(5, 10, "A7"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var seat = await sut.GetSeatByIdAsync(5);

            Assert.NotNull(seat);
            Assert.Equal("A7", seat!.SeatNumber);
        }

        [Fact]
        public async Task GetSeatsByCinemaHallIdAsync_FiltersAndOrders()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.AddRange(
                MakeSeat(1, 10, "C1"),
                MakeSeat(2, 10, "A1"),
                MakeSeat(3, 10, "B1"),
                MakeSeat(4, 10, "A2")
            );
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var list = await sut.GetSeatsByCinemaHallIdAsync(10);

            Assert.Equal(new[] { "A1", "A2", "B1", "C1" }, list.Select(s => s.SeatNumber).ToArray());
        }

        [Fact]
        public async Task AddSeatAsync_Null_Throws()
        {
            using var ctx = NewCtx();
            var sut = new SeatService(ctx);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddSeatAsync(null!));
        }

        [Fact]
        public async Task AddSeatAsync_HallNotExists_Throws()
        {
            using var ctx = NewCtx();
            var sut = new SeatService(ctx);
            var seat = MakeSeat(0, 999, "A1");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddSeatAsync(seat));
            Assert.Contains("CinemaHall #999 does not exist.", ex.Message);
        }

        [Fact]
        public async Task AddSeatAsync_DuplicateInHall_Throws()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.Add(MakeSeat(1, 10, "A1"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var seat = MakeSeat(0, 10, "A1");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddSeatAsync(seat));
            Assert.Contains("A seat with number 'A1' already exists in hall #10", ex.Message);
        }

        [Fact]
        public async Task AddSeatAsync_Success_SetsTimestamps()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            var sut = new SeatService(ctx);

            var seat = MakeSeat(0, 10, "B5");
            var created = await sut.AddSeatAsync(seat);

            Assert.Equal("B5", created.SeatNumber);
            Assert.NotEqual(default, created.CreatedAt);
            Assert.Equal(created.CreatedAt, created.UpdatedAt);
            Assert.Single(ctx.Seat);
        }

        [Fact]
        public async Task UpdateSeatAsync_Null_Throws()
        {
            using var ctx = NewCtx();
            var sut = new SeatService(ctx);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateSeatAsync(null!));
        }

        [Fact]
        public async Task UpdateSeatAsync_NotFound_Throws()
        {
            using var ctx = NewCtx();
            var sut = new SeatService(ctx);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateSeatAsync(MakeSeat(123, 10, "Z9")));
            Assert.Contains("Seat #123 was not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateSeatAsync_ChangeToNonExistingHall_Throws()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.Add(MakeSeat(1, 10, "A1"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var toUpdate = MakeSeat(1, 999, "A1");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateSeatAsync(toUpdate));
            Assert.Contains("CinemaHall #999 does not exist.", ex.Message);
        }

        [Fact]
        public async Task UpdateSeatAsync_ChangeToDuplicateNumber_Throws()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.AddRange(MakeSeat(1, 10, "A1"), MakeSeat(2, 10, "A2"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var toUpdate = MakeSeat(1, 10, "A2");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateSeatAsync(toUpdate));
            Assert.Contains("A seat with number 'A2' already exists in hall #10", ex.Message);
        }

        [Fact]
        public async Task UpdateSeatAsync_Success_WhenChangingNumber()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.Add(MakeSeat(1, 10, "A1"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var toUpdate = MakeSeat(1, 10, "B7");
            toUpdate.SeatType = "VIP";

            var updated = await sut.UpdateSeatAsync(toUpdate);

            Assert.Equal("B7", updated.SeatNumber);
            Assert.Equal("VIP", updated.SeatType);
            Assert.True(updated.UpdatedAt > updated.CreatedAt);
        }

        [Fact]
        public async Task UpdateSeatAsync_Success_WhenChangingHall_NumberUnique()
        {
            using var ctx = NewCtx();
            ctx.Cinema.Add(new Cinema
            {
                Id = 1,
                Name = "CineX",
                Street = "S",
                City = "C",
                State = "ST",
                ZipCode = "0000-000",
                Country = "PT",
                Latitude = 0,
                Longitude = 0,
                CreatedAt = DateTime.UtcNow
            });
            ctx.CinemaHall.AddRange(MakeHall(10, 1), MakeHall(11, 1));
            ctx.Seat.Add(MakeSeat(1, 10, "A1"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var toUpdate = MakeSeat(1, 11, "A1");

            var updated = await sut.UpdateSeatAsync(toUpdate);

            Assert.Equal(11, updated.CinemaHallId);
            Assert.Equal("A1", updated.SeatNumber);
        }

        [Fact]
        public async Task RemoveSeatAsync_NotFound_ReturnsFalse()
        {
            using var ctx = NewCtx();
            var sut = new SeatService(ctx);

            Assert.False(await sut.RemoveSeatAsync(999));
        }

        [Fact]
        public async Task RemoveSeatAsync_Found_ReturnsTrue_AndDeletes()
        {
            using var ctx = NewCtx();
            SeedBasic(ctx);
            ctx.Seat.Add(MakeSeat(1, 10, "A1"));
            await ctx.SaveChangesAsync();

            var sut = new SeatService(ctx);
            var ok = await sut.RemoveSeatAsync(1);

            Assert.True(ok);
            Assert.Empty(ctx.Seat.ToList());
        }
    }
}
