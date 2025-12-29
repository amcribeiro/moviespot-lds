using Microsoft.AspNetCore.Identity;

namespace MovieSpot.Services.Handlers
{
    /// <summary>
    /// Defines methods for securely hashing and verifying passwords using the built-in <see cref="PasswordHasher{TUser}"/>.
    /// 
    /// This interface abstracts password management operations, allowing for easy testing,
    /// dependency injection, and consistent password handling across the application.
    /// </summary>
    public interface IPasswordService
    {
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
        string HashPassword(string password);

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
        bool VerifyPassword(string password, string hashedPassword);
    }
}
