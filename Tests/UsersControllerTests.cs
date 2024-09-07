using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using TaskManagementSystem.Configuration;
using TaskManagementSystem.Controllers;



namespace Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<ILogger<UsersController>> _mockLogger;
        private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private readonly UserService _userService;
        private readonly UsersController _controller;
        private readonly Mock<IUserService> _mockUserService;

        public UsersControllerTests()
        {
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockLogger = new Mock<ILogger<UsersController>>();
            _mockJwtSettings = new Mock<IOptions<JwtSettings>>();

            var jwtSettings = new JwtSettings { Secret = "KMzzE1uKlVqU7sQ0EIHbi5SQNrQrFvftsOlJQl81SbSBZK5MTj8nR5iezeKMHGy2" };
            _mockJwtSettings.Setup(s => s.Value).Returns(jwtSettings);

            _mockUserService = new Mock<IUserService>(); // Mock the IUserService interface

            _controller = new UsersController(_mockLogger.Object, _mockUserService.Object);
        }

        private Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            _mockUserService.Setup(u => u.UserExists(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username or Email already exists.", badRequestResult.Value);

            // Checking the logger call
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Username or Email already exists."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Register_SuccessfulRegistration_ReturnsOk()
        {
            var registerModel = new RegisterModel
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "Password123!"
            };

            _mockUserService.Setup(u => u.UserExists(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            _mockUserService.Setup(u => u.CreateUserAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                            .ReturnsAsync((UserModel user, string password) => user);

            var result = await _controller.Register(registerModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.Equal("User registered successfully.", value["Message"]);

            // Check logging via the common Log method
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"User {registerModel.Username} successfully registered.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Register_ThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            // Mock user creation with error
            _mockUserService.Setup(u => u.CreateUserAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                            .ThrowsAsync(new ArgumentException("Password must contain at least one uppercase letter."));

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password must contain at least one uppercase letter.", badRequestResult.Value);

            // Check logging
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Password must contain at least one uppercase letter.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }


        [Fact]
        public void Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = "validUser",
                Password = "validPassword"
            };

            var user = new UserModel
            {
                Username = "validUser",
                Email = "valid@example.com",
                PasswordHash = "hashedPassword"
            };

            var token = "fake-jwt-token";

            // Mock successful authentication
            _mockUserService.Setup(u => u.Authenticate(loginModel.Username, loginModel.Password)).Returns(user);

            // Mock the generation of the JWT token
            _mockUserService.Setup(u => u.GenerateJwtToken(user)).Returns(token);

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.Equal(token, value["Token"]);

            // Check logging
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"User {user.Username} successfully Authenticated.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = "invalidUser",
                Password = "invalidPassword"
            };

            // We mock that authentication failed
            _mockUserService.Setup(u => u.Authenticate(loginModel.Username, loginModel.Password)).Returns((UserModel)null);

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials.", unauthorizedResult.Value);

            // Check that logging was called with the message "Invalid credentials."
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid credentials.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
