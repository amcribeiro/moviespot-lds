using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Tokens;
using System.Security.Cryptography;

namespace MovieSpot.Tests.Services.Tokens
{
    /// <summary>
    /// Unit tests for <see cref="TokenService"/>.
    /// Verifies the generation and refresh of JWT access tokens and refresh tokens.
    /// </summary>
    public class TokenServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly TokenService _service;

        #region Setup

        public TokenServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var configData = new Dictionary<string, string>
            {
                { "JwtConfig:Issuer", "MovieSpot" },
                { "JwtConfig:Audience", "MovieSpotUsers" },
                { "JwtConfig:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)) },
                { "JwtConfig:TokenValidityInMinutes", "10" },
                { "JwtConfig:RefreshTokenValidityInMinutes", "60" },
            };

            _config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
            _service = new TokenService(_config, _context);
        }

        #endregion

        #region GenerateTokens()

        [Fact]
        public void GenerateTokens_Should_Return_Valid_Tokens()
        {
            var user = new User { Id = 1, Email = "user@test.com", Role = "User" };

            var result = _service.GenerateTokens(user);

            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.Single(_context.RefreshTokens);
        }

        [Fact]
        public void GenerateTokens_Should_Throw_When_Issuer_Or_Audience_Missing()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Key", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("supersecretkeysupersecretkeysupersecretkey1234")) },
                    { "JwtConfig:TokenValidityInMinutes", "10" },
                    { "JwtConfig:RefreshTokenValidityInMinutes", "60" }
                })
                .Build();

            var badService = new TokenService(badConfig, _context);
            var user = new User { Id = 1, Email = "user@test.com", Role = "User" };

            var ex = Assert.Throws<InvalidOperationException>(() => badService.GenerateTokens(user));
            Assert.Contains("Issuer or Audience are not configured", ex.Message);
        }

        [Fact]
        public void GenerateTokens_Should_Throw_InvalidOperationException_When_Key_Too_Short()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Issuer", "MovieSpot" },
                    { "JwtConfig:Audience", "Users" },
                    { "JwtConfig:Key", Convert.ToBase64String(new byte[] { 1 }) },
                    { "JwtConfig:TokenValidityInMinutes", "10" },
                    { "JwtConfig:RefreshTokenValidityInMinutes", "60" }
                })
                .Build();

            var badService = new TokenService(badConfig, _context);
            var user = new User { Id = 1, Email = "user@test.com", Role = "User" };

            var ex = Assert.Throws<InvalidOperationException>(() => badService.GenerateTokens(user));
            Assert.Contains("The JWT key must be at least 512 bits long", ex.Message);
        }

        [Fact]
        public void GenerateTokens_Should_Throw_DbUpdateException_When_SaveFails()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingDbContext(options);

            var user = new User { Id = 1, Email = "user@test.com", Role = "User" };
            failingContext.User.Add(user);
            failingContext.SaveChanges();

            failingContext.ShouldFail = true;

            var service = new TokenService(_config, failingContext);

            var ex = Assert.Throws<DbUpdateException>(() => service.GenerateTokens(user));
            Assert.Contains("An error occurred while saving the refresh token to the database", ex.Message);
        }

        [Fact]
        public void GenerateTokens_Should_Throw_When_User_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.GenerateTokens(null!));
        }

        [Fact]
        public void GenerateTokens_Should_Throw_When_Key_Missing_Or_Invalid()
        {
            var missingKeyConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Issuer", "Issuer" },
                    { "JwtConfig:Audience", "Audience" }
                })
                .Build();

            var serviceMissingKey = new TokenService(missingKeyConfig, _context);
            var user = new User { Id = 1, Email = "user@test.com", Role = "User" };
            Assert.Throws<InvalidOperationException>(() => serviceMissingKey.GenerateTokens(user));

            var invalidKeyConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Issuer", "MovieSpot" },
                    { "JwtConfig:Audience", "Users" },
                    { "JwtConfig:Key", "Invalid@@@NotBase64" },
                    { "JwtConfig:TokenValidityInMinutes", "10" },
                    { "JwtConfig:RefreshTokenValidityInMinutes", "60" },
                })
                .Build();

            var serviceInvalidKey = new TokenService(invalidKeyConfig, _context);
            Assert.Throws<InvalidOperationException>(() => serviceInvalidKey.GenerateTokens(user));

            var shortKeyConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Issuer", "MovieSpot" },
                    { "JwtConfig:Audience", "Users" },
                    { "JwtConfig:Key", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("short")) },
                    { "JwtConfig:TokenValidityInMinutes", "10" },
                    { "JwtConfig:RefreshTokenValidityInMinutes", "60" },
                })
                .Build();

            var serviceShortKey = new TokenService(shortKeyConfig, _context);
            Assert.Throws<InvalidOperationException>(() => serviceShortKey.GenerateTokens(user));
        }

        #endregion

        #region RefreshAccessToken()

        [Fact]
        public void RefreshAccessToken_Should_Return_New_AccessToken_And_Revoke_Old()
        {
            var user = new User { Id = 1, Email = "user@refresh.com", Role = "User" };
            _context.User.Add(user);
            _context.SaveChanges();

            var (_, refresh) = _service.GenerateTokens(user);

            var (newAccess, new_refresh) = _service.RefreshAccessToken(refresh);

            Assert.False(string.IsNullOrWhiteSpace(newAccess));
            Assert.Equal(2, _context.RefreshTokens.Count());
            Assert.True(_context.RefreshTokens.First().IsRevoked);
        }

        [Fact]
        public void RefreshAccessToken_Should_Throw_When_Token_Is_Null_Or_Invalid()
        {
            Assert.Throws<ArgumentNullException>(() => _service.RefreshAccessToken(null!));
            Assert.Throws<UnauthorizedAccessException>(() => _service.RefreshAccessToken("invalidtoken"));
        }

        [Fact]
        public void RefreshAccessToken_Should_Throw_When_Token_Expired()
        {
            var user = new User { Id = 2, Email = "expired@x.com", Role = "User" };
            _context.User.Add(user);
            _context.SaveChanges();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = "expiredtoken",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
                IsRevoked = false
            });
            _context.SaveChanges();

            Assert.Throws<UnauthorizedAccessException>(() => _service.RefreshAccessToken("expiredtoken"));
        }

        [Fact]
        public void RefreshAccessToken_Should_Throw_When_User_Not_Found()
        {
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = 999,
                Token = "tokenwithoutuser",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsRevoked = false
            });
            _context.SaveChanges();

            Assert.Throws<InvalidOperationException>(() => _service.RefreshAccessToken("tokenwithoutuser"));
        }

        [Fact]
        public void RefreshAccessToken_Should_Throw_DbUpdateException_When_SaveFails()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingDbContext(options);

            var user = new User { Id = 1, Email = "user@test.com", Role = "User" };
            failingContext.User.Add(user);
            failingContext.SaveChanges();

            var validToken = new RefreshToken
            {
                UserId = user.Id,
                Token = "valid_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            failingContext.RefreshTokens.Add(validToken);
            failingContext.SaveChanges();

            failingContext.ShouldFail = true;

            var service = new TokenService(_config, failingContext);

            var ex = Assert.Throws<DbUpdateException>(() => service.RefreshAccessToken("valid_refresh_token"));
            Assert.Contains("An error occurred while updating the refresh token state in the database", ex.Message);
        }

        #endregion

        /// <summary>
        /// Simulated DbContext that throws a database error only when <see cref="ShouldFail"/> is set to true.
        /// </summary>
        private class FailingDbContext : ApplicationDbContext
        {
            public bool ShouldFail { get; set; } = false;

            public FailingDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options) { }

            public override int SaveChanges()
            {
                if (ShouldFail)
                    throw new DbUpdateException("Error saving in simulated context.");
                return base.SaveChanges();
            }
        }
    }
}
