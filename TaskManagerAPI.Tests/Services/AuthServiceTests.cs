using Moq;
using TaskManagerAPI.Application.Dtos.Auth;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Services;
using TaskManagerAPI.Infrastructure.Utils;

namespace TaskManagerAPI.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterAsync_NewUser_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new UserRegisterDto
        {
            Email = "test@test.com",
            Username = "testuser",
            Password = "Password123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _emailServiceMock.Setup(x => x.SendConfirmationEmailAsync(request.Email, It.IsAny<int>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.RequiresEmailConfirmation);
        Assert.Contains("successful", result.Message);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendConfirmationEmailAsync(request.Email, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterAsync_ExistingUnconfirmedUser_ResendsConfirmationCode()
    {
        // Arrange
        var request = new UserRegisterDto
        {
            Email = "test@test.com",
            Username = "testuser",
            Password = "Password123!"
        };

        var existingUser = new User
        {
            Email = request.Email,
            IsEmailConfirmed = false
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _emailServiceMock.Setup(x => x.SendConfirmationEmailAsync(request.Email, It.IsAny<int>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.RequiresEmailConfirmation);
        Assert.Contains("sent", result.Message);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendConfirmationEmailAsync(request.Email, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterAsync_ExistingConfirmedUser_ThrowsValidationException()
    {
        // Arrange
        var request = new UserRegisterDto
        {
            Email = "test@test.com",
            Username = "testuser",
            Password = "Password123!"
        };

        var existingUser = new User
        {
            Email = request.Email,
            IsEmailConfirmed = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(request));
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var request = new UserLoginDto
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        byte[] hash, salt;
        PasswordHasher.CreatePasswordHash(request.Password, out hash, out salt);

        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsEmailConfirmed = true
        };

        var accessToken = "access_token";
        var refreshToken = "refresh_token";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateTokenPair(user))
            .Returns((accessToken, refreshToken));
        _refreshTokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _tokenServiceMock.Verify(x => x.GenerateTokenPair(user), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginAsync_InvalidCredentials_ThrowsForbiddenAccessException()
    {
        // Arrange
        var request = new UserLoginDto
        {
            Email = "test@test.com",
            Password = "WrongPassword!"
        };

        byte[] hash, salt;
        PasswordHasher.CreatePasswordHash("CorrectPassword!", out hash, out salt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsEmailConfirmed = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _authService.LoginAsync(request));
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginAsync_UnconfirmedEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new UserLoginDto
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        byte[] hash, salt;
        PasswordHasher.CreatePasswordHash(request.Password, out hash, out salt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsEmailConfirmed = false
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _authService.LoginAsync(request));
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RefreshTokenAsync_ValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var refreshToken = "old_refresh_token";
        var userId = 1;
        var accessToken = "new_access_token";
        var newRefreshToken = "new_refresh_token";

        var token = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken
        };

        var user = new User { Id = userId };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(token);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _refreshTokenRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<RefreshToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _tokenServiceMock.Setup(x => x.GenerateTokenPair(user))
            .Returns((accessToken, newRefreshToken));
        _refreshTokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(newRefreshToken, result.RefreshToken);
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(refreshToken), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<RefreshToken>()), Times.Once);
        _tokenServiceMock.Verify(x => x.GenerateTokenPair(user), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RefreshTokenAsync_InvalidToken_ThrowsForbiddenAccessException()
    {
        // Arrange
        var refreshToken = "invalid_token";

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken)null);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _authService.RefreshTokenAsync(refreshToken));
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task ConfirmEmailAsync_ValidCode_ReturnsSuccessResponse()
    {
        // Arrange
        var code = 123456;
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            IsEmailConfirmed = false,
            EmailConfirmationCode = code
        };

        _userRepositoryMock.Setup(x => x.GetByConfirmationCodeAsync(code))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _authService.ConfirmEmailAsync(code);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("successfully", result.Message);
        _userRepositoryMock.Verify(x => x.GetByConfirmationCodeAsync(code), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task ConfirmEmailAsync_InvalidCode_ThrowsValidationException()
    {
        // Arrange
        var code = 999999;

        _userRepositoryMock.Setup(x => x.GetByConfirmationCodeAsync(code))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _authService.ConfirmEmailAsync(code));
        _userRepositoryMock.Verify(x => x.GetByConfirmationCodeAsync(code), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RevokeTokensAsync_ValidUserId_DeletesTokens()
    {
        // Arrange
        var userId = 1;
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Id = 1, UserId = userId },
            new RefreshToken { Id = 2, UserId = userId }
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(tokens);
        _refreshTokenRepositoryMock.Setup(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _authService.RevokeTokensAsync(userId);

        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
    }
}