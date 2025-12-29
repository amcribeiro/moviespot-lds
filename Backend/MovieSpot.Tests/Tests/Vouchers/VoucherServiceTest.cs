using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Vouchers;
using Xunit;

namespace MovieSpot.Tests.Services.Vouchers
{
    public class VoucherServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly VoucherService _service;

        public VoucherServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new VoucherService(_context);
        }

        #region CreateVoucher

        [Fact]
        public void CreateVoucher_Should_Create_New_Voucher_With_Valid_Fields()
        {
            var voucher = _service.CreateVoucher();

            Assert.NotNull(voucher);
            Assert.False(string.IsNullOrWhiteSpace(voucher.Code));
            Assert.Equal(12, voucher.Code.Length);
            Assert.InRange(voucher.Value, 0.01m, 0.99m);
            Assert.True(voucher.ValidUntil > DateTime.UtcNow);
            Assert.Equal(0, voucher.Usages);

            var fromDb = _context.Voucher.Single(v => v.Id == voucher.Id);
            Assert.Equal(voucher.Code, fromDb.Code);
        }

        [Fact]
        public void CreateVoucher_Should_Persist_Single_Row()
        {
            var voucher = _service.CreateVoucher();

            Assert.Single(_context.Voucher);
            Assert.Equal(voucher.Id, _context.Voucher.First().Id);
        }

        #endregion

        #region GetVoucherById

        [Fact]
        public void GetVoucherById_Should_Return_Correct_Voucher()
        {
            var v = new Voucher
            {
                Code = "TESTCODE000001",
                Value = 0.25m,
                ValidUntil = DateTime.UtcNow.AddDays(10),
                MaxUsages = 5,
                Usages = 0
            };
            _context.Voucher.Add(v);
            _context.SaveChanges();

            var found = _service.GetVoucherById(v.Id);

            Assert.Equal(v.Code, found.Code);
            Assert.Equal(0.25m, found.Value);
        }

        [Fact]
        public void GetVoucherById_Should_Throw_When_Id_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetVoucherById(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetVoucherById(-1));
        }

        [Fact]
        public void GetVoucherById_Should_Throw_When_Not_Found()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetVoucherById(9999));
        }

        #endregion

        #region UpdateVoucher

        [Fact]
        public void UpdateVoucher_Should_Increment_Usages_And_Update_Fields()
        {
            var existing = new Voucher
            {
                Code = "EDITME00000001",
                Value = 0.10m,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                MaxUsages = 2,
                Usages = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            _context.Voucher.Add(existing);
            _context.SaveChanges();

            var update = new Voucher
            {
                Id = existing.Id,
                Code = "EDITME00000002",
                Value = 0.20m,
                ValidUntil = DateTime.UtcNow.AddDays(30)
            };

            var beforeUpdateAt = existing.UpdatedAt;

            _service.UpdateVoucher(update);

            var db = _context.Voucher.Find(existing.Id)!;
            Assert.Equal(2, db.Usages);
            Assert.Equal("EDITME00000002", db.Code);
            Assert.Equal(0.20m, db.Value);
            Assert.True(db.ValidUntil > DateTime.UtcNow.AddDays(15));
            Assert.True(db.UpdatedAt >= beforeUpdateAt);
        }

        [Fact]
        public void UpdateVoucher_Should_Throw_When_Voucher_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.UpdateVoucher(null!));
        }

        [Fact]
        public void UpdateVoucher_Should_Throw_When_Id_Invalid()
        {
            var v = new Voucher { Id = 0, Code = "X", Value = 0.1m, ValidUntil = DateTime.UtcNow.AddDays(1) };
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateVoucher(v));
        }

        [Fact]
        public void UpdateVoucher_Should_Throw_When_Not_Found()
        {
            var v = new Voucher { Id = 999, Code = "X", Value = 0.1m, ValidUntil = DateTime.UtcNow.AddDays(1) };
            Assert.Throws<KeyNotFoundException>(() => _service.UpdateVoucher(v));
        }

        [Fact]
        public void UpdateVoucher_Should_Throw_When_ValidUntil_Is_Past_Or_Present()
        {
            var existing = new Voucher
            {
                Code = "DATEFAIL000000",
                Value = 0.10m,
                ValidUntil = DateTime.UtcNow.AddDays(10),
                MaxUsages = 10,
                Usages = 0
            };
            _context.Voucher.Add(existing);
            _context.SaveChanges();

            var update = new Voucher
            {
                Id = existing.Id,
                Code = existing.Code,
                Value = 0.15m,
                ValidUntil = DateTime.UtcNow
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateVoucher(update));
        }

        [Fact]
        public void UpdateVoucher_Should_Throw_When_Value_Is_Out_Of_Range()
        {
            var existing = new Voucher
            {
                Code = "VALFAIL0000000",
                Value = 0.10m,
                ValidUntil = DateTime.UtcNow.AddDays(10),
                MaxUsages = 10,
                Usages = 0
            };
            _context.Voucher.Add(existing);
            _context.SaveChanges();

            var bad1 = new Voucher
            {
                Id = existing.Id,
                Code = existing.Code,
                Value = 0m,
                ValidUntil = DateTime.UtcNow.AddDays(1)
            };

            var bad2 = new Voucher
            {
                Id = existing.Id,
                Code = existing.Code,
                Value = 1.0m,
                ValidUntil = DateTime.UtcNow.AddDays(1)
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateVoucher(bad1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateVoucher(bad2));
        }

        [Fact]
        public void UpdateVoucher_Should_Throw_When_MaxUsages_Reached()
        {
            var existing = new Voucher
            {
                Code = "MAXEDOUT000000",
                Value = 0.30m,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                MaxUsages = 2,
                Usages = 2
            };
            _context.Voucher.Add(existing);
            _context.SaveChanges();

            var update = new Voucher
            {
                Id = existing.Id,
                Code = existing.Code,
                Value = 0.35m,
                ValidUntil = DateTime.UtcNow.AddDays(31)
            };

            Assert.Throws<InvalidOperationException>(() => _service.UpdateVoucher(update));
        }

        #endregion

        #region Exceptions

        private class ThrowingDbContext : ApplicationDbContext
        {
            private readonly Exception _toThrow;
            public ThrowingDbContext(DbContextOptions<ApplicationDbContext> options, Exception toThrow)
                : base(options) => _toThrow = toThrow;

            public override int SaveChanges() => throw _toThrow;

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                => Task.FromException<int>(_toThrow);
        }

        private static DbContextOptions<ApplicationDbContext> InMemoryOptions(string name) =>
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;

        [Fact]
        public async Task CreateVoucher_Should_Wrap_DbUpdateException_WithoutVirtualOrMoq()
        {
            var dbName = Guid.NewGuid().ToString();
            var throwingCtx = new ThrowingDbContext(
                InMemoryOptions(dbName),
                new DbUpdateException("db low-level"));

            var service = new VoucherService(throwingCtx);

            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => Task.Run(() => service.CreateVoucher()));
            Assert.Contains("Error creating the voucher in the database.", ex.Message);
            Assert.IsType<DbUpdateException>(ex.InnerException);
        }

        [Fact]
        public async Task CreateVoucher_Should_Wrap_GenericException_WithoutVirtualOrMoq()
        {
            var dbName = Guid.NewGuid().ToString();
            var throwingCtx = new ThrowingDbContext(
                InMemoryOptions(dbName),
                new InvalidOperationException("boom"));

            var service = new VoucherService(throwingCtx);

            var ex = await Assert.ThrowsAsync<Exception>(() => Task.Run(() => service.CreateVoucher()));
            Assert.Contains("An unexpected error occurred while creating the voucher.", ex.Message);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public void UpdateVoucher_Should_Wrap_DbUpdateException_WithoutVirtualOrMoq()
        {
            var dbName = Guid.NewGuid().ToString();

            using (var seedCtx = new ApplicationDbContext(InMemoryOptions(dbName)))
            {
                seedCtx.Voucher.Add(new Voucher
                {
                    Id = 123,
                    Code = "OKOKOKOKOKOK",
                    Value = 0.25m,
                    ValidUntil = DateTime.UtcNow.AddDays(5),
                    MaxUsages = 10,
                    Usages = 0
                });
                seedCtx.SaveChanges();
            }

            var throwingCtx = new ThrowingDbContext(
                InMemoryOptions(dbName),
                new DbUpdateException("db low-level"));

            var service = new VoucherService(throwingCtx);

            var update = new Voucher
            {
                Id = 123,
                Code = "OKOKOKOKOKOK",
                Value = 0.25m,
                ValidUntil = DateTime.UtcNow.AddDays(5)
            };

            var ex = Assert.Throws<DbUpdateException>(() => service.UpdateVoucher(update));
            Assert.Contains("Error updating the voucher in the database.", ex.Message);
            Assert.IsType<DbUpdateException>(ex.InnerException);
        }

        [Fact]
        public void UpdateVoucher_Should_Wrap_GenericException_WithoutVirtualOrMoq()
        {
            var dbName = Guid.NewGuid().ToString();

            using (var seedCtx = new ApplicationDbContext(InMemoryOptions(dbName)))
            {
                seedCtx.Voucher.Add(new Voucher
                {
                    Id = 123,
                    Code = "OKOKOKOKOKOK",
                    Value = 0.25m,
                    ValidUntil = DateTime.UtcNow.AddDays(5),
                    MaxUsages = 10,
                    Usages = 0
                });
                seedCtx.SaveChanges();
            }

            var throwingCtx = new ThrowingDbContext(
                InMemoryOptions(dbName),
                new InvalidOperationException("boom"));

            var service = new VoucherService(throwingCtx);

            var update = new Voucher
            {
                Id = 123,
                Code = "OKOKOKOKOKOK",
                Value = 0.25m,
                ValidUntil = DateTime.UtcNow.AddDays(5)
            };

            var ex = Assert.Throws<Exception>(() => service.UpdateVoucher(update));
            Assert.Contains("An unexpected error occurred while updating the voucher.", ex.Message);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        #endregion
    }
}
