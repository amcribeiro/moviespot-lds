using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieSpot.Controllers;
using MovieSpot.DTO_s;
using MovieSpot.Models;
using MovieSpot.Services.Tokens;
using MovieSpot.Services.Users;

namespace MovieSpot.Tests.Controllers.Users
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _controller = new UserController(_userServiceMock.Object, _tokenServiceMock.Object);
        }

        #region GET BY ID

        [Fact]
        public void GetById_Should_Return_Ok_When_User_Exists()
        {
            var user = new User { Id = 1, Name = "Ana", Email = "ana@test.com" };
            _userServiceMock.Setup(s => s.GetUserById(1)).Returns(user);

            var result = _controller.GetById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Ana", ((UserDTO.UserResponseDto)result.Value).Name);
        }

        [Fact]
        public void GetById_Should_Return_400_When_Id_Invalid()
        {
            _userServiceMock.Setup(s => s.GetUserById(It.IsAny<int>()))
                .Throws(new ArgumentOutOfRangeException());

            var result = _controller.GetById(-1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void GetById_Should_Return_404_When_User_NotFound()
        {
            _userServiceMock.Setup(s => s.GetUserById(It.IsAny<int>()))
                .Throws(new KeyNotFoundException("Not found"));

            var result = _controller.GetById(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region GET ALL

        [Fact]
        public void GetAll_Should_Return_Ok_With_Users()
        {
            _userServiceMock.Setup(s => s.GetAllUsers()).Returns(new List<User>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            });

            var result = _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, ((IEnumerable<UserDTO.UserResponseDto>)result.Value).Count());
        }

        [Fact]
        public void GetAll_Should_Return_404_When_Empty()
        {
            _userServiceMock.Setup(s => s.GetAllUsers())
                .Throws(new InvalidOperationException("Empty"));

            var result = _controller.GetAll() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region CREATE

        [Fact]
        public void Create_Should_Return_Ok_When_User_Created()
        {

            var dto = new UserDTO.UserCreateDto
            {
                Name = "João",
                Email = "joao@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            var createdUser = new User
            {
                Id = 1,
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                Phone = dto.Phone,
                AccountStatus = "Active"
            };

            _userServiceMock.Setup(s => s.CreateUser(It.IsAny<User>())).Returns(createdUser);

            var result = _controller.Create(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var response = Assert.IsType<UserDTO.UserResponseDto>(result.Value);
            Assert.Equal("João", response.Name);
            Assert.Equal("User", response.Role);
            Assert.Equal("Active", response.AccountStatus);
        }


        [Fact]
        public void Create_Should_Return_400_When_Service_Throws()
        {
            var dto = new UserDTO.UserCreateDto { Name = "João", Email = "joao@test.com", Password = "123" };

            _userServiceMock.Setup(s => s.CreateUser(It.IsAny<User>()))
                .Throws(new InvalidOperationException("Duplicate"));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Create_Should_Return_400_When_Dto_Is_Null()
        {

            var result = _controller.Create(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("User can't be null.", result.Value);
        }

        [Fact]
        public void Create_Should_Return_400_When_Service_Throws_ArgumentNullException()
        {
            var dto = new UserDTO.UserCreateDto
            {
                Name = "Ana",
                Email = "ana@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.CreateUser(It.IsAny<User>()))
                .Throws(new ArgumentNullException("User", "O utilizador não pode ser nulo."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("O utilizador não pode ser nulo", result.Value!.ToString());
        }

        [Fact]
        public void Create_Should_Return_400_When_Service_Throws_ArgumentException()
        {
            var dto = new UserDTO.UserCreateDto
            {
                Name = "Ana",
                Email = "ana@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.CreateUser(It.IsAny<User>()))
                .Throws(new ArgumentException("O email do utilizador é obrigatório."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("O email do utilizador é obrigatório", result.Value!.ToString());
        }

        [Fact]
        public void Create_Should_Return_400_When_Service_Throws_InvalidOperationException()
        {
            var dto = new UserDTO.UserCreateDto
            {
                Name = "João",
                Email = "joao@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.CreateUser(It.IsAny<User>()))
                .Throws(new InvalidOperationException("Já existe um utilizador com este email."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Já existe um utilizador", result.Value!.ToString());
        }

        [Fact]
        public void Create_Should_Return_400_When_Service_Throws_DbUpdateException()
        {
            var dto = new UserDTO.UserCreateDto
            {
                Name = "Maria",
                Email = "maria@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.CreateUser(It.IsAny<User>()))
                .Throws(new DbUpdateException("Erro ao guardar o novo utilizador."));

            var result = _controller.Create(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Erro ao guardar o novo utilizador", result.Value!.ToString());
        }

        #endregion

        #region REGISTER

        [Fact]
        public void Register_Should_Return_Tokens_When_Successful()
        {

            var dto = new UserDTO.UserCreateDto
            {
                Name = "Maria",
                Email = "maria@test.com",
                Password = "abc",
                Role = "User",
                Phone = "999999999"
            };

            var tokens = new LoginResponseModel
            {
                Email = "maria@test.com",
                AccessToken = "token123",
                RefreshToken = "refresh123"
            };

            _userServiceMock.Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Returns(tokens);

            var result = _controller.Register(dto).Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var response = Assert.IsType<LoginResponseModel>(result.Value);
            Assert.Equal("maria@test.com", response.Email);
        }


        [Fact]
        public void Register_Should_Return_400_When_User_Already_Exists()
        {
            var dto = new UserDTO.UserCreateDto { Name = "Maria", Email = "maria@test.com", Password = "abc" };

            _userServiceMock.Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Throws(new InvalidOperationException("Duplicate"));

            var result = _controller.Register(dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void Register_Should_Return_500_When_Unexpected_Error()
        {
            var dto = new UserDTO.UserCreateDto
            {
                Name = "Maria",
                Email = "maria@test.com",
                Password = "abc",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock.Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Throws(new Exception("Erro inesperado"));

            var result = _controller.Register(dto).Result as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Erro interno", result.Value!.ToString());
        }

        [Fact]
        public void Register_Should_Return_400_When_Request_Is_Null()
        {

            var actionResult = _controller.Register(null);
            var result = actionResult.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("User data cannot be null.", result.Value);
        }

        [Fact]
        public void Register_Should_Return_400_When_Service_Throws_ArgumentNullException()
        {

            var dto = new UserDTO.UserCreateDto
            {
                Name = "Ana",
                Email = "ana@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Throws(new ArgumentNullException("user", "O utilizador não pode ser nulo."));

            var result = _controller.Register(dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("O utilizador não pode ser nulo", result.Value!.ToString());
        }

        [Fact]
        public void Register_Should_Return_400_When_Service_Throws_ArgumentException()
        {

            var dto = new UserDTO.UserCreateDto
            {
                Name = "Bruno",
                Email = "bruno@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Throws(new ArgumentException("A password é obrigatória."));

            var result = _controller.Register(dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("A password é obrigatória", result.Value!.ToString());
        }

        [Fact]
        public void Register_Should_Return_400_When_Service_Throws_InvalidOperationException()
        {

            var dto = new UserDTO.UserCreateDto
            {
                Name = "Carlos",
                Email = "carlos@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Throws(new InvalidOperationException("Já existe um utilizador com este email."));

            var result = _controller.Register(dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Já existe um utilizador", result.Value!.ToString());
        }

        [Fact]
        public void Register_Should_Return_400_When_Service_Throws_DbUpdateException()
        {

            var dto = new UserDTO.UserCreateDto
            {
                Name = "Diana",
                Email = "diana@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.RegisterUser(It.IsAny<User>()))
                .Throws(new DbUpdateException("Erro ao registar o novo utilizador."));

            var result = _controller.Register(dto).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Erro ao registar o novo utilizador", result.Value!.ToString());
        }

        #endregion

        #region UPDATE

        [Fact]
        public void Update_Should_Return_Ok_When_Successful()
        {

            var dto = new UserDTO.UserUpdateDto
            {
                Name = "Joana",
                Email = "joana@test.com",
                Role = "User",
                Phone = "999999999"
            };

            var updated = new User { Id = 1, Name = "Joana", Email = "joana@test.com" };

            _userServiceMock.Setup(s => s.UpdateUser(1, It.IsAny<User>())).Returns(updated);

            var result = _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void Update_Should_Return_404_When_NotFound()
        {

            var dto = new UserDTO.UserUpdateDto
            {
                Name = "Joana",
                Email = "joana@test.com",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<int>(), It.IsAny<User>()))
                .Throws(new KeyNotFoundException("Not found"));

            var result = _controller.Update(1, dto) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void Update_Should_Return_400_When_Dto_Is_Null()
        {

            var result = _controller.Update(1, null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("User can't be null.", result.Value);
        }

        [Fact]
        public void Update_Should_Return_400_When_Validation_Errors_Exist()
        {

            var dto = new UserDTO.UserUpdateDto
            {
                Name = "Tiago",
                Email = "invalid-email",
                Password = "123",
                Role = "User"
            };

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.IsAssignableFrom<IEnumerable<string>>(result.Value);
        }

        [Fact]
        public void Update_Should_Return_400_When_ArgumentOutOfRangeException_Is_Thrown()
        {
            var dto = new UserDTO.UserUpdateDto
            {
                Name = "Carla",
                Email = "carla@test.com",
                Password = "123",
                Role = "User",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.UpdateUser(It.IsAny<int>(), It.IsAny<User>()))
                .Throws(new ArgumentOutOfRangeException("id", "O ID deve ser maior que zero."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("O ID deve ser maior que zero", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_ArgumentNullException_Is_Thrown()
        {
            var dto = new UserDTO.UserUpdateDto
            {
                Name = "Inês",
                Email = "ines@test.com",
                Password = "123",
                Role = "Staff",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.UpdateUser(It.IsAny<int>(), It.IsAny<User>()))
                .Throws(new ArgumentNullException("updatedUser", "O utilizador atualizado não pode ser nulo."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("O utilizador atualizado não pode ser nulo", result.Value!.ToString());
        }

        [Fact]
        public void Update_Should_Return_400_When_DbUpdateException_Is_Thrown()
        {
            var dto = new UserDTO.UserUpdateDto
            {
                Name = "Miguel",
                Email = "miguel@test.com",
                Password = "123",
                Role = "Staff",
                Phone = "999999999"
            };

            _userServiceMock
                .Setup(s => s.UpdateUser(It.IsAny<int>(), It.IsAny<User>()))
                .Throws(new DbUpdateException("Erro ao atualizar o utilizador na base de dados."));

            var result = _controller.Update(1, dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Erro ao atualizar o utilizador na base de dados", result.Value!.ToString());
        }

        #endregion

        #region DELETE

        [Fact]
        public void Delete_Should_Return_Ok_When_Successful()
        {
            var user = new User { Id = 1, Name = "Zé" };
            _userServiceMock.Setup(s => s.DeleteUser(1)).Returns(user);

            var result = _controller.Delete(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void Delete_Should_Return_404_When_NotFound()
        {
            _userServiceMock.Setup(s => s.DeleteUser(It.IsAny<int>()))
                .Throws(new KeyNotFoundException("Not found"));

            var result = _controller.Delete(1) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void Delete_Should_Return_400_When_Id_Is_Invalid()
        {

            _userServiceMock
                .Setup(s => s.DeleteUser(It.IsAny<int>()))
                .Throws(new ArgumentOutOfRangeException("id", "O ID do utilizador deve ser maior que zero."));

            var result = _controller.Delete(0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("O ID do utilizador deve ser maior que zero", result.Value!.ToString());
        }

        [Fact]
        public void Delete_Should_Return_400_When_DbUpdateException_Is_Thrown()
        {

            _userServiceMock
                .Setup(s => s.DeleteUser(It.IsAny<int>()))
                .Throws(new DbUpdateException("Erro ao eliminar o utilizador da base de dados."));

            var result = _controller.Delete(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Erro ao eliminar o utilizador da base de dados", result.Value!.ToString());
        }

        #endregion

        #region LOGIN

        [Fact]
        public void Login_Should_Return_Tokens_When_Valid()
        {
            var request = new LoginRequestModel { Email = "test@test.com", Password = "123" };
            var response = new LoginResponseModel { Email = "test@test.com", AccessToken = "abc" };

            _userServiceMock.Setup(s => s.LoginUser(request.Email, request.Password)).Returns(response);

            var result = _controller.Login(request).Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void Login_Should_Return_401_When_Invalid_Credentials()
        {
            _userServiceMock.Setup(s => s.LoginUser(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new UnauthorizedAccessException("Credenciais inválidas"));

            var result = _controller.Login(new LoginRequestModel { Email = "x", Password = "y" }).Result as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void Login_Should_Return_400_When_ArgumentNullException_Is_Thrown()
        {
            var request = new LoginRequestModel
            {
                Email = "",
                Password = ""
            };

            _userServiceMock
                .Setup(s => s.LoginUser(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentNullException("Email e password são obrigatórios para login."));

            var result = _controller.Login(request).Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Email e password são obrigatórios para login", result.Value!.ToString());
        }

        [Fact]
        public void Login_Should_Return_500_When_Unexpected_Exception_Is_Thrown()
        {

            var request = new LoginRequestModel
            {
                Email = "teste@teste.com",
                Password = "123"
            };

            _userServiceMock
                .Setup(s => s.LoginUser(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Falha inesperada"));

            var result = _controller.Login(request).Result as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Erro interno: Falha inesperada", result.Value!.ToString());
        }

        #endregion

        #region REFRESH

        [Fact]
        public void Refresh_Should_Return_New_Access_Token()
        {
            _tokenServiceMock.Setup(t => t.RefreshAccessToken("valid-refresh"))
                .Returns(("new-token", "new-refresh-token"));

            var result = _controller.Refresh("valid-refresh") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var value = result.Value;

            var accessTokenProp = value
                .GetType()
                .GetProperty("AccessToken")
                ?.GetValue(value, null)?.ToString();

            Assert.Equal("new-token", accessTokenProp);
        }

        [Fact]
        public void Refresh_Should_Return_401_When_Invalid()
        {
            _tokenServiceMock.Setup(t => t.RefreshAccessToken(It.IsAny<string>()))
                .Throws(new UnauthorizedAccessException("Token inválido"));

            var result = _controller.Refresh("bad-token") as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        #endregion
    }
}