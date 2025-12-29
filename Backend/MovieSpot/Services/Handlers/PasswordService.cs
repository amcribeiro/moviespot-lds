using Microsoft.AspNetCore.Identity;

namespace MovieSpot.Services.Handlers
{
    /// <summary>
    /// Provides secure password hashing and verification using the built-in <see cref="PasswordHasher{TUser}"/>.
    /// 
    /// This class simplifies password management by leveraging the ASP.NET Core Identity hasher,
    /// which uses PBKDF2 with HMAC-SHA256 and automatically handles salts and iterations.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<object> _hasher = new();

        /// <summary>
        /// Generates a secure hash for a given plain-text password using PBKDF2 with an automatically generated salt.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>
        /// A hashed password string that includes the algorithm, version, salt, and derived subkey.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the provided password is null, empty, or consists only of whitespace.
        /// </exception>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");

            return _hasher.HashPassword(null, password);
        }

        /// <summary>
        /// Verifies whether a provided plain-text password matches a previously hashed password.
        /// </summary>
        /// <param name="password">The plain-text password provided by the user for verification.</param>
        /// <param name="hashedPassword">The hashed password stored in the system.</param>
        /// <returns>
        /// <c>true</c> if the password matches the hash; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="password"/> or <paramref name="hashedPassword"/> is null, empty, or consists only of whitespace.
        /// </exception>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                throw new ArgumentNullException(nameof(password), "Password and hashed password are required.");

            var result = _hasher.VerifyHashedPassword(null, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
