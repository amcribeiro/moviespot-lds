using Microsoft.AspNetCore.Identity;
using MovieSpot.Services.Handlers;
using Xunit;

namespace MovieSpot.Tests.Services.Handlers
{
    /// <summary>
    /// Unit tests for the <see cref="PasswordService"/>.
    /// Ensures that password hash creation and verification
    /// work correctly, including error and exception scenarios.
    /// </summary>
    public class PasswordServiceTest
    {
        private readonly PasswordService _service;

        public PasswordServiceTest()
        {
            _service = new PasswordService();
        }

        #region HashPassword()

        [Fact]
        public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
        {
            var password = "StrongPassword123!";

            var hash = _service.HashPassword(password);

            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void HashPassword_NullOrEmpty_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.HashPassword(null!));
            Assert.Throws<ArgumentNullException>(() => _service.HashPassword(""));
            Assert.Throws<ArgumentNullException>(() => _service.HashPassword("   "));
        }

        [Fact]
        public void HashPassword_RepeatedCalls_ReturnDifferentHashes()
        {
            var password = "SamePassword";
            var hash1 = _service.HashPassword(password);
            var hash2 = _service.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
        }

        #endregion

        #region VerifyPassword()

        [Fact]
        public void VerifyPassword_ValidPassword_ReturnsTrue()
        {
            var password = "MySecret!";
            var hash = _service.HashPassword(password);

            Assert.True(_service.VerifyPassword(password, hash));
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var hash = _service.HashPassword("CorrectOne");
            Assert.False(_service.VerifyPassword("WrongOne", hash));
        }

        [Fact]
        public void VerifyPassword_EmptyInputs_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.VerifyPassword(null!, "hash"));
            Assert.Throws<ArgumentNullException>(() => _service.VerifyPassword("", "hash"));
            Assert.Throws<ArgumentNullException>(() => _service.VerifyPassword("password", null!));
            Assert.Throws<ArgumentNullException>(() => _service.VerifyPassword("password", ""));
        }

        [Fact]
        public void VerifyPassword_InvalidHash_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => _service.VerifyPassword("password", "###INVALID###"));
        }

        [Fact]
        public void VerifyPassword_HashTampered_ReturnsFalse()
        {
            var password = "Password123";
            var hash = _service.HashPassword(password);

            var tampered = $"{hash[..^2]}AA";

            Assert.False(_service.VerifyPassword(password, tampered));
        }

        #endregion
    }
}
