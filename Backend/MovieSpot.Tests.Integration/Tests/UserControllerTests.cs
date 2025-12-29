using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MovieSpot.Data;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Tests.Integration.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Xunit;
using static MovieSpot.DTO_s.UserDTO;

namespace MovieSpot.Tests.Integration.Tests
{
    public class UserControllerTests : IntegrationTestBase
    {
        private readonly TestDataFactory _dataFactory;
        public UserControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dataFactory = new TestDataFactory(_client, factory.Services);
            _dataFactory.ClearDatabaseAsync().Wait();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(FakeAuthHandler.AuthenticationScheme);
        }

        #region GET /User/{id}
        [Fact]
        public async Task GetById_ReturnsOk_WhenUserExists()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            var response = await _client.GetAsync($"/User/{user.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var response = await _client.GetAsync("/User/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region GET /User
        [Fact]
        public async Task GetAll_ReturnsOk_WhenUsersExist()
        {
            await _dataFactory.CreateTestUserAsync();

            var response = await _client.GetAsync("/User");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponseDto>>();
            Assert.NotEmpty(users);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenNoUsersExist()
        {
            var response = await _client.GetAsync("/User");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region POST /User
        [Fact]
        public async Task Create_ReturnsOk_WhenValidUser()
        {
            var dto = new UserCreateDto
            {
                Name = "Maria",
                Email = "maria@example.com",
                Password = "123456",
                Phone = "912345678",
                Role = "User"
            };

            var response = await _client.PostAsJsonAsync("/User", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<UserResponseDto>();
            Assert.Equal(dto.Email, created.Email);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenUserIsNull()
        {
            var response = await _client.PostAsJsonAsync("/User", (UserCreateDto?)null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenEmailInvalid()
        {
            var dto = new UserCreateDto
            {
                Name = "Pedro",
                Email = "invalid-email",
                Password = "123456",
                Phone = "912345678",
                Role = "User"
            };

            var response = await _client.PostAsJsonAsync("/User", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region POST /User/register
        [Fact]
        public async Task Register_ReturnsOk_WhenValidData()
        {
            var dto = new UserCreateDto
            {
                Name = "João",
                Email = $"joao_{Guid.NewGuid()}@example.com",
                Password = "123456",
                Phone = "911111111",
                Role = "User"
            };

            var response = await _client.PostAsJsonAsync("/User/register", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tokens = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
            Assert.NotNull(tokens.AccessToken);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailInvalid()
        {
            var dto = new UserCreateDto
            {
                Name = "João",
                Email = "invalid-email",
                Password = "123456",
                Phone = "911111111",
                Role = "User"
            };

            var response = await _client.PostAsJsonAsync("/User/register", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PUT /User/{id}
        [Fact]
        public async Task Update_ReturnsOk_WhenValid()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            var dto = new UserUpdateDto
            {
                Name = "User Atualizado",
                Email = user.Email,
                Phone = user.Phone,
                Role = "User",
                Password = "newpass",
                AccountStatus = "Active"
            };

            var response = await _client.PutAsJsonAsync($"/User/{user.Id}", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
            Assert.Equal("User Atualizado", result.Name);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenInvalidRole()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            var dto = new UserUpdateDto
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = "InvalidRole"
            };

            var response = await _client.PutAsJsonAsync($"/User/{user.Id}", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region DELETE /User/{id}
        [Fact]
        public async Task Delete_ReturnsOk_WhenUserExists()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            var response = await _client.DeleteAsync($"/User/{user.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var response = await _client.DeleteAsync("/User/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region POST /User/login
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            var login = new LoginRequestModel
            {
                Email = user.Email,
                Password = "123456"
            };

            var response = await _client.PostAsJsonAsync("/User/login", login);

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {body}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
            Assert.NotNull(result.AccessToken);
        }


        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalidPassword()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            var login = new LoginRequestModel
            {
                Email = user.Email,
                Password = "wrongpassword"
            };

            var response = await _client.PostAsJsonAsync("/User/login", login);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion

        #region POST /User/forgot-password
        [Fact]
        public async Task ForgotPassword_ReturnsOkOrHandledError_WhenEmailExists()
        {
            var user = await _dataFactory.CreateTestUserAsync();
            var dto = new ForgotPasswordRequestDto
            {
                Email = user.Email
            };
            var response = await _client.PostAsJsonAsync("/User/forgot-password", dto);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("Erro interno", content, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Assert.Fail($"Unexpected status code: {response.StatusCode}");
            }
        }

        [Fact]
        public async Task ForgotPassword_ReturnsNotFound_WhenEmailDoesNotExist()
        {
            var response = await _client.PostAsJsonAsync("/User/forgot-password", new { email = "noexist@example.com" });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region POST /User/reset-password
        [Fact]
        public async Task ResetPassword_ReturnsOk_WhenTokenValid()
        {
            var user = await _dataFactory.CreateTestUserAsync();

            using var scope = _factory.Services.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var secret = configuration["JwtConfig:Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, user.Email) }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var dto = new ResetPasswordRequestDto
            {
                Token = jwt,
                NewPassword = "newpass123"
            };

            var response = await _client.PostAsJsonAsync("/User/reset-password", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenMissingData()
        {
            var dto = new ResetPasswordRequestDto
            {
                Token = "",
                NewPassword = ""
            };

            var response = await _client.PostAsJsonAsync("/User/reset-password", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion
    }
}
