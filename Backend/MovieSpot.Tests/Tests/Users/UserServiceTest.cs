using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Moq;
using MovieSpot.Data;
using MovieSpot.Models;
using MovieSpot.Services.Emails;
using MovieSpot.Services.Handlers;
using MovieSpot.Services.Tokens;
using MovieSpot.Services.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieSpot.Tests.Services.Users
{
    /// <summary>
    /// Unit tests for <see cref="UserService"/>.
    /// Verifies user creation, registration, login, update and deletion,
    /// as well as error scenarios and expected exceptions.
    /// </summary>
    public class UserServiceTest
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly IConfiguration _config;
        private readonly UserService _service;
        private readonly Mock<IEmailService> _emailServiceMock;

        public UserServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _emailServiceMock = new Mock<IEmailService>();

            _tokenServiceMock = new Mock<ITokenService>();
            _tokenServiceMock
                .Setup(t => t.GenerateTokens(It.IsAny<User>()))
                .Returns(("fake-access-token", "fake-refresh-token"));

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Secret", "supersecretkey12345678901234567890" },
                    { "JwtConfig:TokenValidityInMinutes", "15" }
                })
                .Build();

            _service = new UserService(_context, _config, _tokenServiceMock.Object, _emailServiceMock.Object);
        }

        #region CREATE

        [Fact]
        public void CreateUser_Should_Add_New_User()
        {
            var user = new User { Name = "Ana", Email = "ana@test.com", Password = "123" };

            var result = _service.CreateUser(user);

            Assert.NotNull(result);
            Assert.Single(_context.User);
            Assert.NotEqual("123", result.Password);
        }

        [Fact]
        public void CreateUser_Should_Throw_When_User_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.CreateUser(null!));
        }

        [Fact]
        public void CreateUser_Should_Throw_When_Email_Exists()
        {
            _context.User.Add(new User { Name = "João", Email = "joao@test.com", Password = "abc" });
            _context.SaveChanges();

            var user = new User { Name = "Maria", Email = "joao@test.com", Password = "xyz" };

            Assert.Throws<InvalidOperationException>(() => _service.CreateUser(user));
        }

        [Fact]
        public void CreateUser_Should_Throw_When_Email_Is_Empty()
        {
            var user = new User { Name = "SemEmail", Email = "", Password = "123" };
            var ex = Assert.Throws<ArgumentException>(() => _service.CreateUser(user));
            Assert.Contains("User email is required", ex.Message);
        }

        [Fact]
        public void CreateUser_Should_Throw_When_Password_Is_Empty()
        {
            var user = new User { Name = "SemPass", Email = "x@test.com", Password = "" };
            var ex = Assert.Throws<ArgumentException>(() => _service.CreateUser(user));
            Assert.Contains("Password is required", ex.Message);
        }

        [Fact]
        public void CreateUser_Should_Throw_DbUpdateException_When_SaveFails()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingDbContext(options);
            var service = new UserService(failingContext, _config, _tokenServiceMock.Object, _emailServiceMock.Object);

            var user = new User { Name = "Erro", Email = "erro@test.com", Password = "123" };

            failingContext.ShouldFail = true;

            var ex = Assert.Throws<DbUpdateException>(() => service.CreateUser(user));
            Assert.Contains("An error occurred while saving the new user to the database", ex.Message);
        }

        #endregion

        #region REGISTER

        [Fact]
        public void RegisterUser_Should_Create_User_And_Return_Tokens()
        {
            var user = new User { Name = "Carlos", Email = "carlos@test.com", Password = "pass", Role = "User" };

            var result = _service.RegisterUser(user);

            Assert.NotNull(result);
            Assert.Equal("carlos@test.com", result.Email);
            Assert.Equal("fake-access-token", result.AccessToken);
            Assert.Equal("fake-refresh-token", result.RefreshToken);
        }

        [Fact]
        public void RegisterUser_Should_Throw_When_User_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.RegisterUser(null!));
        }

        [Fact]
        public void RegisterUser_Should_Throw_When_Email_Exists()
        {
            _context.User.Add(new User { Name = "Rita", Email = "rita@test.com", Password = "pass" });
            _context.SaveChanges();

            var user = new User { Name = "Nova", Email = "rita@test.com", Password = "nova" };

            Assert.Throws<InvalidOperationException>(() => _service.RegisterUser(user));
        }

        [Fact]
        public void RegisterUser_Should_Throw_When_Email_Is_Empty()
        {
            var user = new User { Name = "SemEmail", Email = "", Password = "123" };
            var ex = Assert.Throws<ArgumentException>(() => _service.RegisterUser(user));
            Assert.Contains("User email is required", ex.Message);
        }

        [Fact]
        public void RegisterUser_Should_Throw_When_Email_Is_Null()
        {
            var user = new User { Name = "SemEmail", Email = null!, Password = "123" };
            var ex = Assert.Throws<ArgumentException>(() => _service.RegisterUser(user));
            Assert.Contains("User email is required", ex.Message);
        }

        [Fact]
        public void RegisterUser_Should_Throw_When_Password_Is_Empty()
        {
            var user = new User { Name = "SemPass", Email = "x@test.com", Password = "" };
            var ex = Assert.Throws<ArgumentException>(() => _service.RegisterUser(user));
            Assert.Contains("Password is required", ex.Message);
        }

        [Fact]
        public void RegisterUser_Should_Throw_When_Password_Is_Null()
        {
            var user = new User { Name = "SemPass", Email = "x@test.com", Password = null! };
            var ex = Assert.Throws<ArgumentException>(() => _service.RegisterUser(user));
            Assert.Contains("Password is required", ex.Message);
        }

        [Fact]
        public void RegisterUser_Should_Throw_DbUpdateException_When_SaveFails()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingDbContext(options)
            {
                ShouldFail = true
            };

            var service = new UserService(failingContext, _config, _tokenServiceMock.Object, _emailServiceMock.Object);

            var user = new User { Name = "Erro", Email = "erro@test.com", Password = "123" };

            var ex = Assert.Throws<DbUpdateException>(() => service.RegisterUser(user));
            Assert.Contains("An error occurred while registering the new user in the database", ex.Message);
        }

        #endregion

        #region LOGIN

        [Fact]
        public void LoginUser_Should_Return_Tokens_When_Credentials_Are_Valid()
        {
            var passwordService = new PasswordService();
            var user = new User
            {
                Name = "Miguel",
                Email = "miguel@test.com",
                Password = passwordService.HashPassword("12345"),
                Role = "User"
            };

            _context.User.Add(user);
            _context.SaveChanges();

            var result = _service.LoginUser("miguel@test.com", "12345");

            Assert.NotNull(result);
            Assert.Equal("fake-access-token", result.AccessToken);
            Assert.Equal("fake-refresh-token", result.RefreshToken);
        }

        [Fact]
        public void LoginUser_Should_Throw_When_Password_Is_Invalid()
        {
            var passwordService = new PasswordService();
            var user = new User
            {
                Name = "Tiago",
                Email = "tiago@test.com",
                Password = passwordService.HashPassword("realpass"),
                Role = "User"
            };

            _context.User.Add(user);
            _context.SaveChanges();

            Assert.Throws<UnauthorizedAccessException>(() => _service.LoginUser("tiago@test.com", "wrongpass"));
        }

        [Fact]
        public void LoginUser_Should_Throw_When_Email_Not_Exists()
        {
            Assert.Throws<UnauthorizedAccessException>(() => _service.LoginUser("naoexiste@test.com", "123"));
        }

        [Fact]
        public void LoginUser_Should_Throw_When_Email_Is_Empty_Or_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.LoginUser("", "123"));
            Assert.Throws<ArgumentNullException>(() => _service.LoginUser(null!, "123"));
        }

        [Fact]
        public void LoginUser_Should_Throw_When_Password_Is_Empty_Or_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.LoginUser("user@test.com", ""));
            Assert.Throws<ArgumentNullException>(() => _service.LoginUser("user@test.com", null!));
        }

        #endregion

        #region GET USER(S)

        [Fact]
        public void GetUserById_Should_Return_User()
        {
            var user = new User { Name = "Sara", Email = "sara@test.com", Password = "xyz" };
            _context.User.Add(user);
            _context.SaveChanges();

            var found = _service.GetUserById(user.Id);

            Assert.Equal("sara@test.com", found.Email);
        }

        [Fact]
        public void GetUserById_Should_Throw_When_Invalid_Id()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetUserById(0));
        }

        [Fact]
        public void GetUserById_Should_Throw_When_Not_Found()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetUserById(123));
        }

        [Fact]
        public void GetAllUsers_Should_Return_List()
        {
            _context.User.AddRange(
                new User { Name = "A", Email = "a@x.com", Password = "1" },
                new User { Name = "B", Email = "b@x.com", Password = "2" }
            );
            _context.SaveChanges();

            var users = _service.GetAllUsers();

            Assert.Equal(2, users.Count());
        }

        [Fact]
        public void GetAllUsers_Should_Throw_When_Empty()
        {
            Assert.Throws<InvalidOperationException>(() => _service.GetAllUsers());
        }

        #endregion

        #region UPDATE

        [Fact]
        public void UpdateUser_Should_Modify_Existing_User()
        {
            var user = new User
            {
                Name = "Paula",
                Email = "paula@test.com",
                Password = "old"
            };
            _context.User.Add(user);
            _context.SaveChanges();

            var updated = new User
            {
                Name = "Paula Updated",
                Email = "paula@test.com",
                Password = "new",
                Role = "Admin"
            };

            var result = _service.UpdateUser(user.Id, updated);

            Assert.Equal("Paula Updated", result.Name);
            Assert.Equal("Admin", result.Role);

            Assert.NotEqual("old", result.Password);

            var passwordService = new PasswordService();
            Assert.True(passwordService.VerifyPassword("new", result.Password));
        }

        [Fact]
        public void UpdateUser_Should_Throw_When_User_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _service.UpdateUser(1, null!));
        }

        [Fact]
        public void UpdateUser_Should_Throw_When_Id_Invalid()
        {
            var user = new User { Name = "Invalid", Email = "x@x.com", Password = "123" };
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.UpdateUser(0, user));
        }

        [Fact]
        public void UpdateUser_Should_Throw_When_User_Not_Found()
        {
            var updated = new User { Name = "Novo", Email = "novo@x.com", Password = "123" };
            Assert.Throws<KeyNotFoundException>(() => _service.UpdateUser(999, updated));
        }

        [Fact]
        public void UpdateUser_Should_Throw_DbUpdateException_When_SaveFails()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingDbContext(options);

            var user = new User
            {
                Name = "Original",
                Email = "original@test.com",
                Password = "123"
            };

            failingContext.User.Add(user);
            failingContext.SaveChanges();

            failingContext.ShouldFail = true;

            var service = new UserService(failingContext, _config, _tokenServiceMock.Object, _emailServiceMock.Object);

            var updated = new User
            {
                Name = "Novo Nome",
                Email = "original@test.com",
                Password = "nova123",
                Role = "Admin"
            };

            var ex = Assert.Throws<DbUpdateException>(() => service.UpdateUser(user.Id, updated));
            Assert.Contains("An error occurred while updating the user in the database", ex.Message);
        }

        #endregion

        #region DELETE USER

        [Fact]
        public void DeleteUser_Should_Remove_User()
        {
            var user = new User { Name = "Pedro", Email = "pedro@test.com", Password = "abc" };
            _context.User.Add(user);
            _context.SaveChanges();

            var deleted = _service.DeleteUser(user.Id);

            Assert.Equal(user.Id, deleted.Id);
            Assert.Empty(_context.User);
        }

        [Fact]
        public void DeleteUser_Should_Throw_When_Id_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.DeleteUser(0));
        }

        [Fact]
        public void DeleteUser_Should_Throw_When_User_Not_Found()
        {
            Assert.Throws<KeyNotFoundException>(() => _service.DeleteUser(999));
        }

        [Fact]
        public void DeleteUser_Should_Throw_DbUpdateException_When_SaveFails()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var failingContext = new FailingDbContext(options);

            var user = new User { Name = "Falha", Email = "falha@test.com", Password = "123" };
            failingContext.User.Add(user);
            failingContext.SaveChanges();

            failingContext.ShouldFail = true;

            var service = new UserService(failingContext, _config, _tokenServiceMock.Object, _emailServiceMock.Object);

            var ex = Assert.Throws<DbUpdateException>(() => service.DeleteUser(user.Id));
            Assert.Contains("An error occurred while deleting the user from the database", ex.Message);
        }

        #endregion

        #region FORGOT / RESET PASSWORD

        [Fact]
        public async Task ForgotPassword_Should_Send_Reset_Link_When_User_Exists()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Key", "supersecretkey12345678901234567890" },
                    { "Frontend:BaseUrl", "https://frontend.test" },
                    { "MailSettings:FromEmail", "noreply@moviespot.com" }
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var user = new User { Name = "Inês", Email = "ines@test.com", Password = "123" };
            context.User.Add(user);
            context.SaveChanges();

            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var result = await service.ForgotPassword("ines@test.com");

            Assert.True(result);
            emailServiceMock.Verify(e => e.SendMimeMessageAsync(It.IsAny<MimeMessage>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_Should_Throw_When_Email_Not_Found()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Key", "supersecretkey12345678901234567890" },
                    { "Frontend:BaseUrl", "https://frontend.test" },
                    { "MailSettings:FromEmail", "noreply@moviespot.com" }
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ForgotPassword("naoexiste@test.com"));
            Assert.Contains("No user found", ex.Message);
        }

        [Fact]
        public async Task ForgotPassword_Should_Throw_When_Email_Is_Null_Or_Empty()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Secret", "supersecretkey12345678901234567890" },
                    { "Frontend:BaseUrl", "https://frontend.test" },
                    { "MailSettings:FromEmail", "noreply@moviespot.com" }
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ForgotPassword(""));
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ForgotPassword(null!));
        }

        [Fact]
        public void ResetPassword_Should_Update_User_Password_When_Token_Is_Valid()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var secretKey = "supersecretkey12345678901234567890";
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "JwtConfig:Key", secretKey } })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var user = new User { Name = "Joana", Email = "joana@test.com", Password = "oldpass" };
            context.User.Add(user);
            context.SaveChanges();

            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "joana@test.com") }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var result = service.ResetPassword(token, "novaPassword123");

            Assert.True(result);
            var updatedUser = context.User.First(u => u.Email == "joana@test.com");
            Assert.NotEqual("oldpass", updatedUser.Password);
        }

        [Fact]
        public void ResetPassword_Should_Return_False_When_Token_Is_Invalid()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "JwtConfig:Key", "supersecretkey12345678901234567890" } })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var result = service.ResetPassword("token-invalido", "123");

            Assert.False(result);
        }

        [Fact]
        public void ResetPassword_Should_Throw_When_Token_Is_Null_Or_Empty()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => _service.ResetPassword(null!, "novaPass123"));
            Assert.Contains("Token is required", ex1.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() => _service.ResetPassword("", "novaPass123"));
            Assert.Contains("Token is required", ex2.Message);
        }

        [Fact]
        public void ResetPassword_Should_Throw_When_NewPassword_Is_Null_Or_Empty()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => _service.ResetPassword("qualquerToken", null!));
            Assert.Contains("New password is required", ex1.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() => _service.ResetPassword("qualquerToken", ""));
            Assert.Contains("New password is required", ex2.Message);
        }

        [Fact]
        public void ResetPassword_Should_Return_False_When_Token_Has_No_Email_Claim()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Key", "supersecretkey12345678901234567890" }
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JwtConfig:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("something", "blah")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var result = service.ResetPassword(token, "NovaPass123!");

            Assert.False(result);
        }

        [Fact]
        public void ResetPassword_Should_Return_False_When_User_Not_Found_For_TokenEmail()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Key", "supersecretkey12345678901234567890" }
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JwtConfig:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, "fantasma@test.com")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            var result = service.ResetPassword(token, "OutraPass123!");
            Assert.False(result);
        }

        [Fact]
        public void ResetPassword_Should_Return_False_When_Token_Is_Expired()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtConfig:Key", "supersecretkey12345678901234567890" }
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var emailServiceMock = new Mock<IEmailService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var service = new UserService(context, config, tokenServiceMock.Object, emailServiceMock.Object);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JwtConfig:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, "alguem@test.com")
                }),
                NotBefore = DateTime.UtcNow.AddMinutes(-10),
                Expires = DateTime.UtcNow.AddMinutes(-5),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var expiredToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            var result = service.ResetPassword(expiredToken, "NovaPass123!");
            Assert.False(result);
        }

        #endregion

        /// <summary>
        /// Simulated DbContext that throws a DbUpdateException in SaveChanges when <see cref="ShouldFail"/> is true.
        /// </summary>
        private class FailingDbContext : ApplicationDbContext
        {
            public bool ShouldFail { get; set; }

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
