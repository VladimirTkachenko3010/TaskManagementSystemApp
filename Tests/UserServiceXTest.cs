using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagementSystem.Configuration;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tests
{
    public class UserServiceXTest
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private readonly UserService _userService;

        public UserServiceXTest()
        {
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockJwtSettings = new Mock<IOptions<JwtSettings>>();

            // Set up a secret key for JWT
            var jwtSettings = new JwtSettings { Secret = "KMzzE1uKlVqU7sQ0EIHbi5SQNrQrFvftsOlJQl81SbSBZK5MTj8nR5iezeKMHGy2" }; // Пример секретного ключа
            _mockJwtSettings.Setup(s => s.Value).Returns(jwtSettings);

            // Create a mock for DbSet<UserModel>
            var mockUserSet = new Mock<DbSet<UserModel>>();

            // Customize the behavior of DbSet<UserModel>
            _mockContext.Setup(m => m.Users).Returns(mockUserSet.Object);

            // Initialize UserService with mocks
            _userService = new UserService(_mockContext.Object, new Mock<IConfiguration>().Object, _mockJwtSettings.Object);
        }


        private Mock<ApplicationDbContext> SetupMockContext(List<UserModel> data)
        {
            var mockSet = new Mock<DbSet<UserModel>>();
            mockSet.As<IQueryable<UserModel>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            mockSet.As<IQueryable<UserModel>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            mockSet.As<IQueryable<UserModel>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            mockSet.As<IQueryable<UserModel>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            return mockContext;
        }

        [Fact]
        public void UserExists_WithExistingUsername_ReturnsTrue()
        {
            // Arrange
            var data = new List<UserModel>
        {
            new UserModel {
                Id = Guid.NewGuid(),
                Username = "existingUser",
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
        };
            var mockContext = SetupMockContext(data);
            var service = new UserService(mockContext.Object);

            // Act
            var result = service.UserExists("existingUser", "newuser@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserExists_WithExistingEmail_ReturnsTrue()
        {
            // Arrange
            var data = new List<UserModel>
        {
            new UserModel {
                Id = Guid.NewGuid(),
                Username = "existingUser",
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
        };
            var mockContext = SetupMockContext(data);
            var service = new UserService(mockContext.Object);

            // Act
            var result = service.UserExists("newuser", "user@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserExists_WithNonExistingUsernameAndEmail_ReturnsFalse()
        {
            // Arrange
            var data = new List<UserModel>
        {
            new UserModel {
                Id = Guid.NewGuid(),
                Username = "existingUser",
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
        };
            var mockContext = SetupMockContext(data);
            var service = new UserService(mockContext.Object);

            // Act
            var result = service.UserExists("newuser", "newuser@example.com");

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task CreateUserAsync_ValidUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new UserModel
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            var password = "ValidPassword123!";

            // Act
            var result = await _userService.CreateUserAsync(user, password);

            // Assert
            _mockContext.Verify(m => m.Users.Add(It.IsAny<UserModel>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task CreateUserAsync_InvalidPassword_ShouldThrowArgumentException()
        {
            // Arrange
            var user = new UserModel
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            var invalidPassword = "short";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.CreateUserAsync(user, invalidPassword));
        }


        [Fact]
        public void Authenticate_ValidUsernameAndPassword_ShouldReturnUser()
        {
            // Arrange
            var testUsername = "testuser";
            var testPassword = "ValidPassword123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(testPassword);

            var userList = new List<UserModel>
        {
            new UserModel { Username = testUsername, Email = "test@example.com", PasswordHash = hashedPassword }
        }.AsQueryable();

            var mockUserSet = new Mock<DbSet<UserModel>>();
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.Provider).Returns(userList.Provider);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);

            // Act
            var result = _userService.Authenticate(testUsername, testPassword);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testUsername, result.Username);
        }

        [Fact]
        public void Authenticate_InvalidUsername_ShouldReturnNull()
        {
            // Arrange
            var testUsername = "nonexistentuser";
            var testPassword = "ValidPassword123";

            var userList = new List<UserModel>
        {
            new UserModel { Username = "testuser", Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(testPassword) }
        }.AsQueryable();

            var mockUserSet = new Mock<DbSet<UserModel>>();
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.Provider).Returns(userList.Provider);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);

            // Act
            var result = _userService.Authenticate(testUsername, testPassword);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Authenticate_InvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var testUsername = "testuser";
            var testPassword = "WrongPassword";
            var correctPassword = "ValidPassword123";

            var userList = new List<UserModel>
        {
            new UserModel { Username = testUsername, Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword) }
        }.AsQueryable();

            var mockUserSet = new Mock<DbSet<UserModel>>();
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.Provider).Returns(userList.Provider);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockUserSet.As<IQueryable<UserModel>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);

            // Act
            var result = _userService.Authenticate(testUsername, testPassword);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("Short1!", "Password must be at least 8 characters long.")]
        [InlineData("nouppercase1!", "Password must contain at least one uppercase letter.")]
        [InlineData("NoDigit!!", "Password must contain at least one digit.")]
        [InlineData("NoSpecial1", "Password must contain at least one special character.")]
        public void ValidatePassword_InvalidPasswords_ShouldReturnFalseAndErrorMessage(string password, string expectedErrorMessage)
        {
            // Act
            var result = UserService.ValidatePassword(password, out string errorMessage);

            // Assert
            Assert.False(result);
            Assert.Equal(expectedErrorMessage, errorMessage);
        }

        [Fact]
        public void ValidatePassword_ValidPassword_ShouldReturnTrue()
        {
            // Arrange
            var validPassword = "ValidPass1!";

            // Act
            var result = UserService.ValidatePassword(validPassword, out string errorMessage);

            // Assert
            Assert.True(result);
            Assert.Equal(string.Empty, errorMessage);
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hashedPassword = _userService.HashPassword(password);

            // Assert
            Assert.NotEqual(password, hashedPassword);
            Assert.True(BCrypt.Net.BCrypt.Verify(password, hashedPassword));
        }

        [Fact]
        public void VerifyPassword_ValidPassword_ShouldReturnTrue()
        {
            // Arrange
            var password = "TestPassword123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Act
            var result = _userService.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_InvalidPassword_ShouldReturnFalse()
        {
            // Arrange
            var password = "TestPassword123!";
            var wrongPassword = "WrongPassword!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Act
            var result = _userService.VerifyPassword(wrongPassword, hashedPassword);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public void GenerateJwtToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new UserModel
            {
                Id = Guid.NewGuid(),
                Username = "testuser"
            };

            // Act
            var token = _userService.GenerateJwtToken(user);

            // Assert
            Assert.NotNull(token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("KMzzE1uKlVqU7sQ0EIHbi5SQNrQrFvftsOlJQl81SbSBZK5MTj8nR5iezeKMHGy2"); // Use the same key as in the JWT settings

            // Check the validity of the token
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var usernameClaim = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            // Проверяем содержимое токена
            Assert.Equal(user.Id.ToString(), userIdClaim);
            Assert.Equal(user.Username, usernameClaim);
        }

    }
}
