using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Emails;
using MovieSpot.Services.Handlers;
using MovieSpot.Services.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieSpot.Services.Users
{
    /// <summary>
    /// Provides services for managing users in the system,
    /// including creation, update, deletion, authentication and retrieval.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly PasswordService _passwordService;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The database context used to interact with user data.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="tokenService">Service used to generate authentication tokens.</param>
        /// <param name="emailService">Service used to send emails.</param>
        public UserService(ApplicationDbContext context, IConfiguration configuration, ITokenService tokenService, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _tokenService = tokenService;
            _emailService = emailService;
            _passwordService = new PasswordService();
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="newUser">The user object containing the details for creation.</param>
        /// <returns>The newly created user object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided user object is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the email or password is empty or null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a user with the same email already exists.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving changes to the database.</exception>
        public User CreateUser(User newUser)
        {
            if (newUser == null)
                throw new ArgumentNullException(nameof(newUser), "User cannot be null.");

            if (string.IsNullOrWhiteSpace(newUser.Email))
                throw new ArgumentException("User email is required", nameof(newUser.Email));

            if (string.IsNullOrWhiteSpace(newUser.Password))
                throw new ArgumentException("Password is required", nameof(newUser.Password));

            if (_context.User.Any(u => u.Email == newUser.Email))
                throw new InvalidOperationException("A user with this email already exists.");

            try
            {
                newUser.Password = _passwordService.HashPassword(newUser.Password);

                _context.User.Add(newUser);
                _context.SaveChanges();

                return newUser;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while saving the new user to the database", ex);
            }
        }

        /// <summary>
        /// Registers a new user in the system and automatically generates authentication tokens.
        /// </summary>
        /// <param name="newUser">The user object containing the details for registration.</param>
        /// <returns>
        /// A <see cref="LoginResponseModel"/> containing the access and refresh tokens for the newly registered user.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided user object is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the email or password fields are empty or invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a user with the same email already exists.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving the user to the database.</exception>
        public LoginResponseModel RegisterUser(User newUser)
        {
            if (newUser == null)
                throw new ArgumentNullException(nameof(newUser), "User cannot be null.");

            if (string.IsNullOrWhiteSpace(newUser.Email))
                throw new ArgumentException("User email is required", nameof(newUser.Email));

            if (string.IsNullOrWhiteSpace(newUser.Password))
                throw new ArgumentException("Password is required", nameof(newUser.Password));

            if (_context.User.Any(u => u.Email == newUser.Email))
                throw new InvalidOperationException("A user with this email already exists.");

            try
            {
                newUser.Password = _passwordService.HashPassword(newUser.Password);

                _context.User.Add(newUser);
                _context.SaveChanges();

                var tokens = _tokenService.GenerateTokens(newUser);

                var tokenExpiry = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtConfig:TokenValidityInMinutes")
                );

                return new LoginResponseModel
                {
                    Email = newUser.Email,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    ExpiresIn = (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds
                };
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while registering the new user in the database", ex);
            }
        }

        /// <summary>
        /// Deletes a user from the system based on their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to be deleted.</param>
        /// <returns>The deleted user object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified user is not found.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving changes to the database.</exception>
        public User DeleteUser(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "User ID must be greater than zero.");

            var user = _context.User.Find(id);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            try
            {
                _context.User.Remove(user);
                _context.SaveChanges();
                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while deleting the user from the database", ex);
            }
        }

        /// <summary>
        /// Retrieves all registered users from the system.
        /// </summary>
        /// <returns>A collection of all users.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no users are found in the database.</exception>
        public IEnumerable<User> GetAllUsers()
        {
            var users = _context.User.ToList();

            if (users == null || !users.Any())
                throw new InvalidOperationException("No users are registered in the system.");

            return users;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>The user associated with the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified user is not found.</exception>
        public User GetUserById(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "User ID must be greater than zero.");

            var user = _context.User.Find(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found.");

            return user;
        }

        /// <summary>
        /// Authenticates a user based on their email and password.
        /// </summary>
        /// <param name="email">The email of the user attempting to log in.</param>
        /// <param name="password">The password of the user attempting to log in.</param>
        /// <returns>A <see cref="LoginResponseModel"/> with tokens if credentials are valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when email or password are null or empty.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
        public LoginResponseModel LoginUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("Email and password are required for login.");

            var userAccount = _context.User.SingleOrDefault(u => u.Email == email);
            if (userAccount is null || !_passwordService.VerifyPassword(password, userAccount.Password))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var tokens = _tokenService.GenerateTokens(userAccount);

            var tokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtConfig:TokenValidityInMinutes"));

            return new LoginResponseModel
            {
                Email = userAccount.Email,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresIn = (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds
            };
        }

        /// <summary>
        /// Updates the details of an existing user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to be updated.</param>
        /// <param name="updatedUser">The user object containing the updated details.</param>
        /// <returns>The updated user object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the updated user object is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the ID is less than or equal to zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified user is not found.</exception>
        /// <exception cref="DbUpdateException">Thrown when an error occurs while saving changes to the database.</exception>
        public User UpdateUser(int id, User updatedUser)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "User ID must be greater than zero.");

            if (updatedUser == null)
                throw new ArgumentNullException(nameof(updatedUser), "Updated user cannot be null.");

            var user = _context.User.Find(id);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.AccountStatus = updatedUser.AccountStatus;
            user.Phone = updatedUser.Phone;
            user.Role = updatedUser.Role;
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                user.Password = _passwordService.HashPassword(updatedUser.Password);
            }

            try
            {
                _context.User.Update(user);
                _context.SaveChanges();
                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while updating the user in the database", ex);
            }
        }

        /// <summary>
        /// Initiates the password recovery process by generating a temporary reset token 
        /// and sending a password reset link to the user’s registered email address.
        /// </summary>
        /// <param name="email">The email address of the user requesting the password reset.</param>
        /// <returns>
        /// <c>true</c> if the password reset email was successfully sent; otherwise, an exception is thrown.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the provided <paramref name="email"/> is null, empty, or consists only of whitespace.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no user is found associated with the provided email address.
        /// </exception>
        /// <remarks>
        /// The generated token is a JWT containing the user's email claim and is valid for 15 minutes.
        /// A password reset link containing this token is sent via email using the configured SMTP service.
        /// </remarks>
        public async Task<bool> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email is required.");

            var user = _context.User.FirstOrDefault(u => u.Email == email);
            if (user == null)
                throw new KeyNotFoundException("No user found with this email.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var resetLink = $"{_configuration["Frontend:BaseUrl"]}/reset-password?token={token}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MovieSpot", _configuration["MailSettings:FromEmail"]));
            message.To.Add(new MailboxAddress(user.Name, user.Email));
            message.Subject = "Reset Password - MovieSpot";

            message.Body = new TextPart("plain")
            {
                Text = $"Olá {user.Name},\n\nClick on the link to redefine your password:\n{resetLink}\n\nthe link expires in 15 seconds.\n\nTeam Moviespot"
            };
            await _emailService.SendMimeMessageAsync(message);

            return true;
        }

        /// <summary>
        /// Resets a user's password using a valid password reset token previously generated by <see cref="ForgotPassword(string)"/>.
        /// </summary>
        /// <param name="token">The JWT token received by the user for password reset validation.</param>
        /// <param name="newPassword">The new password to be set for the user account.</param>
        /// <returns>
        /// <c>true</c> if the password was successfully reset; otherwise, <c>false</c> (for invalid/expired tokens or unknown users).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="token"/> or <paramref name="newPassword"/> is null, empty, or consists only of whitespace.
        /// </exception>
        /// <remarks>
        /// This method validates the JWT token, retrieves the associated user based on the email claim,
        /// hashes the new password using <see cref="PasswordService"/>, and updates the user record in the database.
        /// If the token is invalid, expired, has no email claim, or the user does not exist, the method returns <c>false</c>.
        /// </remarks>
        public bool ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token), "Token is required");
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentNullException(nameof(newPassword), "New password is required");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                }, out SecurityToken validatedToken);

                var email = principal.FindFirstValue(ClaimTypes.Email);
                if (email == null)
                    throw new SecurityTokenException("Invalid or expired token.");

                var user = _context.User.FirstOrDefault(u => u.Email == email);
                if (user == null)
                    throw new KeyNotFoundException("User not found.");

                user.Password = _passwordService.HashPassword(newPassword);
                _context.User.Update(user);
                _context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public List<TopUserDto> GetTopUsers(int topN)
        {
            return _context.User
                .Select(u => new TopUserDto
                {
                    UserName = u.Name,
                    Email = u.Email,
                    TotalBookings = u.Bookings.Count(b => b.Status == true)
                })
                .OrderByDescending(u => u.TotalBookings)
                .Take(topN)
                .ToList();
        }

        public List<TopSpenderDto> GetTopSpenders(int count)
        {
            return _context.User
                .Select(u => new TopSpenderDto
                {
                    UserId = u.Id,
                    UserName = u.Name,

                    // Mantemos a contagem de reservas confirmadas
                    TotalBookings = u.Bookings.Count(b => b.Status == true),

                    // ALTERAÇÃO AQUI: 
                    // Navegamos de Booking -> Payment para somar o que foi realmente pago.
                    // Verificamos se o Payment não é null e se o status é "Paid".
                    TotalSpent = u.Bookings
                        .Where(b => b.Payment != null && b.Payment.PaymentStatus == "Paid")
                        .Sum(b => b.Payment.AmountPaid)
                })
                .OrderByDescending(dto => dto.TotalSpent)
                .Take(count)
                .ToList();
        }
    }
}
