using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Tokens;
using MovieSpot.Services.Users;
using System.Text.RegularExpressions;
using static MovieSpot.DTO_s.UserDTO;

namespace MovieSpot.Controllers
{
    /// <summary>
    /// Controller responsible for user management endpoints.
    /// Returns 200 on success, 400 for invalid input/operation,
    /// and 404 when the user is not found.
    /// </summary>

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">User service abstraction.</param>

        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public UserController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>The matching <see cref="User"/>.</returns>
        /// <response code="200">User found successfully.</response>
        /// <response code="400">Invalid ID supplied.</response>
        /// <response code="404">User not found.</response>

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(UserDTO.UserResponseDto), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                var response = new UserDTO.UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    AccountStatus = user.AccountStatus,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>List of users.</returns>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="404">No users found.</response>

        [HttpGet]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(IEnumerable<UserDTO.UserResponseDto>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetAll()
        {
            try
            {
                var users = _userService.GetAllUsers();
                var response = users.Select(u => new UserDTO.UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    AccountStatus = u.AccountStatus,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                });

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="newUser">User payload.</param>
        /// <returns>The created user.</returns>
        /// <response code="200">User created successfully.</response>
        /// <response code="400">Invalid payload or email already exists.</response>

        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(UserDTO.UserResponseDto), 200)]
        [ProducesResponseType(400)]
        public IActionResult Create([FromBody] UserDTO.UserCreateDto newUserDto)
        {
            if (newUserDto is null)
                return BadRequest("User can't be null.");

            var errors = ValidateUser(newUserDto.Email, newUserDto.Password, newUserDto.Role, newUserDto.Phone, requirePassword: true);
            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                var newUser = new User
                {
                    Name = newUserDto.Name,
                    Email = newUserDto.Email,
                    Password = newUserDto.Password,
                    Phone = newUserDto.Phone ?? string.Empty,
                    Role = newUserDto.Role,
                    AccountStatus = "Active"
                };

                var created = _userService.CreateUser(newUser);

                var response = new UserDTO.UserResponseDto
                {
                    Id = created.Id,
                    Name = created.Name,
                    Email = created.Email,
                    Phone = created.Phone,
                    Role = created.Role,
                    AccountStatus = created.AccountStatus,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt
                };

                return Ok(response);
            }

            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Registers a new user and returns authentication tokens.
        /// </summary>
        /// <param name="request">The user registration payload.</param>
        /// <returns>
        /// A <see cref="LoginResponseModel"/> containing the access and refresh tokens for the newly registered user.
        /// </returns>
        /// <response code="200">User registered successfully and tokens returned.</response>
        /// <response code="400">Invalid payload or user already exists.</response>
        /// <response code="500">Unexpected error while registering the user.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponseModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public ActionResult<LoginResponseModel> Register([FromBody] UserDTO.UserCreateDto request)
        {
            if (request is null)
                return BadRequest("User data cannot be null.");

            var errors = ValidateUser(request.Email, request.Password, request.Role, request.Phone, requirePassword: true);
            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                var newUser = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    Phone = request.Phone ?? string.Empty,
                    Role = request.Role,
                    AccountStatus = "Active"
                };

                var result = _userService.RegisterUser(newUser);

                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user by ID.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>The deleted user.</returns>
        /// <response code="200">User deleted successfully.</response>
        /// <response code="400">Invalid ID supplied.</response>
        /// <response code="404">User not found.</response>

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "User,Staff")]
        [ProducesResponseType(typeof(UserDTO.UserResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            try
            {
                var deleted = _userService.DeleteUser(id);
                var response = new UserDTO.UserResponseDto
                {
                    Id = deleted.Id,
                    Name = deleted.Name,
                    Email = deleted.Email,
                    Phone = deleted.Phone,
                    Role = deleted.Role,
                    AccountStatus = deleted.AccountStatus,
                    CreatedAt = deleted.CreatedAt,
                    UpdatedAt = deleted.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="updatedUser">Updated user payload.</param>
        /// <returns>The updated user.</returns>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Invalid ID or payload.</response>
        /// <response code="404">User not found.</response>

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserDTO.UserResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "User,Staff")]
        public IActionResult Update(int id, [FromBody] UserDTO.UserUpdateDto updatedDto)
        {
            if (updatedDto is null)
                return BadRequest("User can't be null.");

            var errors = ValidateUser(updatedDto.Email, updatedDto.Password, updatedDto.Role, updatedDto.Phone, requirePassword: false);
            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                var updatedUser = new User
                {
                    Name = updatedDto.Name,
                    Email = updatedDto.Email,
                    Password = updatedDto.Password ?? string.Empty,
                    Phone = updatedDto.Phone ?? string.Empty,
                    Role = updatedDto.Role,
                    AccountStatus = updatedDto.AccountStatus ?? "Active"
                };

                var result = _userService.UpdateUser(id, updatedUser);

                var response = new UserDTO.UserResponseDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    Email = result.Email,
                    Phone = result.Phone,
                    Role = result.Role,
                    AccountStatus = result.AccountStatus,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<LoginResponseModel> Login([FromBody] LoginRequestModel request)
        {
            try
            {
                var result = _userService.LoginUser(request.Email, request.Password);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public ActionResult Refresh([FromBody] string refreshToken)
        {
            try
            {
                var (accessToken, refreshTokenNew) = _tokenService.RefreshAccessToken(refreshToken);

                return Ok(new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenNew
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Initiates the password recovery process for a user by sending a reset link via email.
        /// </summary>
        /// <param name="email">The email address of the user requesting password recovery.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with a success message if the email was sent successfully,
        /// or <see cref="BadRequestObjectResult"/> / <see cref="NotFoundObjectResult"/> if validation fails.
        /// </returns>
        /// <response code="200">Password reset link sent successfully to the user's email.</response>
        /// <response code="400">Invalid email format or missing email parameter.</response>
        /// <response code="404">No user found with the provided email address.</response>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ForgotPassword([FromBody] UserDTO.ForgotPasswordRequestDto request)

        {

            var email = request?.Email;

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("O email é obrigatório.");

            try
            {
                var result = await _userService.ForgotPassword(email);
                if (result)
                    return Ok("O link para redefinir a password foi enviado com sucesso para o teu email.");
                else
                    return StatusCode(500, "Ocorreu um erro ao tentar enviar o email.");
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the user's password using a valid token previously sent via email.
        /// </summary>
        /// <param name="request">The payload containing the reset token and the new password.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> when the password is successfully reset,
        /// or <see cref="BadRequestObjectResult"/> if the token or password is invalid.
        /// </returns>
        /// <response code="200">Password successfully reset.</response>
        /// <response code="400">Invalid token or password provided.</response>
        /// <response code="401">Token has expired or is invalid.</response>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult ResetPassword([FromBody] UserDTO.ResetPasswordRequestDto request)
        {
            if (request == null)
                return BadRequest("O corpo do pedido não pode ser nulo.");

            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("O token e a nova password são obrigatórios.");

            try
            {
                var result = _userService.ResetPassword(request.Token, request.NewPassword);

                if (!result)
                    return Unauthorized("Token inválido ou expirado.");

                return Ok("Password redefinida com sucesso.");
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna o top X utilizadores com mais reservas confirmadas.
        /// </summary>
        /// <param name="count">Número de utilizadores a retornar (default: 5).</param>
        [HttpGet("stats/top-users")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<TopUserDto>> GetTopUsers([FromQuery] int count)
        {
            try
            {
                var topUsers = _userService.GetTopUsers(count);
                return Ok(topUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("stats/top-spenders")]
        [Authorize(Roles = "Staff")]
        public ActionResult<List<TopSpenderDto>> GetTopSpenders([FromQuery] int count = 5)
        {
            try
            {
                var result = _userService.GetTopSpenders(count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Validates the user model with basic rules:
        /// Email is required and must be valid;
        /// Password required (when specified);
        /// Optional mobile pattern for Cellphone.
        /// </summary>
        private List<string> ValidateUser(string email, string? password, string role, string? phone, bool requirePassword)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
                errors.Add("Email is required.");
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Email format is invalid.");

            if (requirePassword && string.IsNullOrWhiteSpace(password))
                errors.Add("Password is required.");

            if (string.IsNullOrWhiteSpace(role))
                errors.Add("Role is required (e.g. Staff, User).");

            var validRoles = new[] { "Staff", "User" };
            if (!validRoles.Contains(role))
                errors.Add("Invalid role. Only Staff and User are accepted.");

            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (!Regex.IsMatch(phone, @"^\d{9}$"))
                    errors.Add("Phone number must have 9 digits.");

                if (!Regex.IsMatch(phone, @"^(9|2)\d{8}$"))
                    errors.Add("Invalid Portuguese phone number format.");
            }

            return errors;
        }
    }
}
