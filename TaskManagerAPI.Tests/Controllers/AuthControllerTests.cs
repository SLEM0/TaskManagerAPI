//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Moq;
//using System.Security.Claims;
//using TaskManagerAPI.Application.Dtos.Auth;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Controllers;
//using TaskManagerAPI.Domain.Entities;
//using TaskManagerAPI.Infrastructure.Data;

//namespace TaskManagerAPI.Tests.Controllers
//{
//    public class AuthControllerTests : IDisposable
//    {
//        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
//        private readonly AppDbContext _dbContext;
//        private readonly Mock<ITokenService> _mockTokenService;
//        private readonly Mock<ILogger<AuthController>> _mockLogger;
//        private readonly AuthController _controller;

//        public AuthControllerTests()
//        {
//            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            _dbContext = new AppDbContext(_dbContextOptions);
//            _mockTokenService = new Mock<ITokenService>();
//            _mockLogger = new Mock<ILogger<AuthController>>();

//            _controller = new AuthController(_dbContext, _mockTokenService.Object, _mockLogger.Object);
//        }

//        public void Dispose()
//        {
//            _dbContext.Dispose();
//        }

//        #region Register Tests

//        [Fact]
//        public async System.Threading.Tasks.Task Register_WithValidData_ReturnsSuccess()
//        {
//            // Arrange
//            var request = new UserRegisterDto
//            {
//                Username = "testuser",
//                Email = "test@example.com",
//                Password = "Password123!",
//                ConfirmPassword = "Password123!"
//            };

//            // Act
//            var actionResult = await _controller.Register(request);

//            // Assert
//            var result = actionResult.Result as OkObjectResult;
//            Assert.NotNull(result);

//            var response = result.Value as RegisterResponseDto;
//            Assert.NotNull(response);
//            Assert.Equal("User registered successfully.", response.Message);

//            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
//            Assert.NotNull(user);
//            Assert.Equal("testuser", user.Username);
//        }

//        [Fact]
//        public async System.Threading.Tasks.Task Register_WithExistingEmail_ReturnsBadRequest()
//        {
//            // Arrange
//            var existingUser = new User
//            {
//                Username = "existing",
//                Email = "existing@example.com",
//                PasswordHash = new byte[64],
//                PasswordSalt = new byte[128]
//            };
//            _dbContext.Users.Add(existingUser);
//            await _dbContext.SaveChangesAsync();

//            var request = new UserRegisterDto
//            {
//                Username = "newuser",
//                Email = "existing@example.com",
//                Password = "Password123!",
//                ConfirmPassword = "Password123!"
//            };

//            // Act
//            var actionResult = await _controller.Register(request);

//            // Assert
//            var result = actionResult.Result as BadRequestObjectResult;
//            Assert.NotNull(result);

//            var response = result.Value as ErrorResponseDto;
//            Assert.NotNull(response);
//            Assert.Equal("User already exists.", response.Error);
//        }

//        #endregion

//        #region Login Tests

//        [Fact]
//        public async System.Threading.Tasks.Task Login_WithValidCredentials_ReturnsTokens()
//        {
//            // 1. Arrange - создаем реальные хеш и соль
//            const string testPassword = "correct_password";
//            byte[] realHash, realSalt;

//            // Используем реальную реализацию для создания хеша
//            using (var hmac = new System.Security.Cryptography.HMACSHA512())
//            {
//                realSalt = hmac.Key;
//                realHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(testPassword));
//            }

//            // 2. Создаем и сохраняем пользователя
//            var testUser = new User
//            {
//                Id = 1,
//                Email = "test@example.com",
//                Username = "testuser",
//                PasswordHash = realHash,
//                PasswordSalt = realSalt
//            };

//            _dbContext.Users.Add(testUser);
//            await _dbContext.SaveChangesAsync();

//            // 3. Настраиваем мок для токен-сервиса
//            _mockTokenService.Setup(x => x.GenerateTokenPair(It.Is<User>(u => u.Id == testUser.Id)))
//                .Returns(("access_token", "refresh_token"));

//            // 4. Настраиваем проверку пароля (упрощенная версия)
//            PasswordHasher.VerifyPasswordHash = (password, hash, salt) =>
//            {
//                // Простая проверка - пароль должен совпадать с testPassword
//                return password == testPassword;
//            };

//            // 5. Создаем запрос
//            var request = new UserLoginDto
//            {
//                Email = "test@example.com",
//                Password = testPassword
//            };

//            // 6. Act
//            var actionResult = await _controller.Login(request);

//            // 7. Assert
//            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//            var response = Assert.IsType<AuthResponseDto>(okResult.Value);

//            Assert.Equal("access_token", response.AccessToken);
//            Assert.Equal("refresh_token", response.RefreshToken);
//            Assert.Equal(900, response.ExpiresIn);
//        }

//        [Fact]
//        public async System.Threading.Tasks.Task Login_WithInvalidCredentials_ReturnsBadRequest()
//        {
//            // Arrange
//            var user = new User
//            {
//                Username = "testuser",
//                Email = "test@example.com",
//                PasswordHash = new byte[64],
//                PasswordSalt = new byte[128]
//            };
//            _dbContext.Users.Add(user);
//            await _dbContext.SaveChangesAsync();

//            var request = new UserLoginDto
//            {
//                Email = "test@example.com",
//                Password = "wrong_password"
//            };

//            PasswordHasher.VerifyPasswordHash = (password, hash, salt) => false;

//            // Act
//            var actionResult = await _controller.Login(request);

//            // Assert
//            var result = actionResult.Result as BadRequestObjectResult;
//            Assert.NotNull(result);

//            var response = result.Value as ErrorResponseDto;
//            Assert.NotNull(response);
//            Assert.Equal("Invalid credentials.", response.Error);
//        }

//        #endregion

//        #region RefreshToken Tests

//        [Fact]
//        public async System.Threading.Tasks.Task RefreshToken_WithValidToken_ReturnsNewTokens()
//        {
//            // Arrange
//            var user = new User
//            {
//                Username = "testuser",
//                Email = "test@example.com",
//                PasswordHash = new byte[64],
//                PasswordSalt = new byte[128]
//            };
//            _dbContext.Users.Add(user);

//            var oldRefreshToken = new RefreshToken
//            {
//                Token = "old_refresh_token",
//                User = user,
//                Expires = DateTime.UtcNow.AddDays(1)
//            };
//            _dbContext.RefreshTokens.Add(oldRefreshToken);
//            await _dbContext.SaveChangesAsync();

//            _mockTokenService.Setup(x => x.GenerateTokenPair(It.IsAny<User>()))
//                .Returns(("new_access_token", "new_refresh_token"));

//            var request = new RefreshTokenDto { RefreshToken = "old_refresh_token" };

//            // Act
//            var actionResult = await _controller.RefreshToken(request);

//            // Assert
//            var result = actionResult.Result as OkObjectResult;
//            Assert.NotNull(result);

//            var response = result.Value as AuthResponseDto;
//            Assert.NotNull(response);
//            Assert.Equal("new_access_token", response.AccessToken);
//            Assert.Equal("new_refresh_token", response.RefreshToken);

//            var oldToken = await _dbContext.RefreshTokens.FindAsync(oldRefreshToken.Id);
//            Assert.Null(oldToken);

//            var newToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync();
//            Assert.NotNull(newToken);
//            Assert.Equal("new_refresh_token", newToken.Token);
//        }

//        [Fact]
//        public async System.Threading.Tasks.Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
//        {
//            // Arrange
//            var user = new User
//            {
//                Username = "testuser",
//                Email = "test@example.com",
//                PasswordHash = new byte[64],
//                PasswordSalt = new byte[128]
//            };
//            _dbContext.Users.Add(user);

//            var expiredRefreshToken = new RefreshToken
//            {
//                Token = "expired_token",
//                User = user,
//                Expires = DateTime.UtcNow.AddDays(-1)
//            };
//            _dbContext.RefreshTokens.Add(expiredRefreshToken);
//            await _dbContext.SaveChangesAsync();

//            var request = new RefreshTokenDto { RefreshToken = "expired_token" };

//            // Act
//            var actionResult = await _controller.RefreshToken(request);

//            // Assert
//            var result = actionResult.Result as UnauthorizedObjectResult;
//            Assert.NotNull(result);

//            var response = result.Value as ErrorResponseDto;
//            Assert.NotNull(response);
//            Assert.Equal("Invalid refresh token", response.Error);
//        }

//        #endregion

//        #region Revoke Tests

//        [Fact]
//        public async System.Threading.Tasks.Task Revoke_WhenAuthenticated_RemovesAllTokens()
//        {
//            // Arrange
//            var userId = 1;
//            var user = new User
//            {
//                Id = userId,
//                Username = "testuser",
//                Email = "test@example.com",
//                PasswordHash = new byte[64],
//                PasswordSalt = new byte[128]
//            };
//            _dbContext.Users.Add(user);

//            var tokens = new List<RefreshToken>
//            {
//                new RefreshToken { Token = "token1", UserId = userId, Expires = DateTime.UtcNow.AddDays(1) },
//                new RefreshToken { Token = "token2", UserId = userId, Expires = DateTime.UtcNow.AddDays(1) }
//            };
//            _dbContext.RefreshTokens.AddRange(tokens);
//            await _dbContext.SaveChangesAsync();

//            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
//            var identity = new ClaimsIdentity(claims, "Test");
//            var claimsPrincipal = new ClaimsPrincipal(identity);
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
//            };

//            // Act
//            var result = await _controller.Revoke();

//            // Assert
//            Assert.IsType<NoContentResult>(result);
//            var remainingTokens = await _dbContext.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();
//            Assert.Empty(remainingTokens);
//        }

//        #endregion

//        private static class PasswordHasher
//        {
//            public static Func<string, byte[], byte[], bool> VerifyPasswordHash { get; set; } =
//                (password, hash, salt) => true;

//            public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
//            {
//                hash = new byte[64];
//                salt = new byte[128];
//            }
//        }
//    }
//}