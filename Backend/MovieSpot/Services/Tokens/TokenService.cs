using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieSpot.Data;
using MovieSpot.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MovieSpot.Services.Tokens
{
    /// <summary>
    /// Provides functionality for generating and refreshing JWT access tokens and refresh tokens.
    /// Includes built-in validation and secure storage of refresh tokens in the database.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration containing JWT settings.</param>
        /// <param name="context">The database context for persisting refresh tokens.</param>
        public TokenService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Generates a new pair of tokens (access and refresh) for a given user.
        /// </summary>
        public (string AccessToken, string RefreshToken) GenerateTokens(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            try
            {
                var issuer = _configuration["JwtConfig:Issuer"];
                var audience = _configuration["JwtConfig:Audience"];
                var keyConfig = _configuration["JwtConfig:Key"];
                var tokenValidity = _configuration.GetValue<int>("JwtConfig:TokenValidityInMinutes");
                var refreshValidity = _configuration.GetValue<int>("JwtConfig:RefreshTokenValidityInMinutes");

                if (string.IsNullOrWhiteSpace(keyConfig))
                    throw new InvalidOperationException("The JWT key (JwtConfig:Key) is not configured.");

                if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                    throw new InvalidOperationException("Issuer or Audience are not configured in JwtConfig.");

                var keyBytes = Encoding.UTF8.GetBytes(keyConfig);
                if (keyBytes.Length < 64)
                    throw new InvalidOperationException("The JWT key must be at least 512 bits long (64 bytes).");

                var claims = new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha512);

                var jwtToken = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(tokenValidity),
                    signingCredentials: creds
                );

                var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                var refreshEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(refreshValidity),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _context.RefreshTokens.Add(refreshEntity);
                _context.SaveChanges();

                return (accessToken, refreshToken);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("The JWT key in the configuration file is not valid Base64.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("An error occurred while generating the JWT tokens.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while saving the refresh token to the database.", ex);
            }
        }

        /// <summary>
        /// Refreshes the access token using a valid refresh token.
        /// </summary>
        public (string new_acesstoken, string new_refreshtoken) RefreshAccessToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty.");

            try
            {
                var stored = _context.RefreshTokens
                    .FirstOrDefault(r => r.Token == refreshToken && !r.IsRevoked);

                if (stored == null)
                    throw new UnauthorizedAccessException("The provided refresh token is invalid or has already been revoked.");

                if (stored.ExpiresAt < DateTime.UtcNow)
                    throw new UnauthorizedAccessException("The refresh token has expired.");

                var user = _context.User.Find(stored.UserId);
                if (user == null)
                    throw new InvalidOperationException("The user associated with the refresh token was not found.");

                stored.IsRevoked = true;
                _context.SaveChanges();

                return GenerateTokens(user);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the refresh token state in the database.", ex);
            }
        }
    }
}
