using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Services;


namespace TaskManagerAPI.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        SetupConfiguration();
        _tokenService = new TokenService(_mockConfig.Object);
    }

    private void SetupConfiguration()
    {
        _mockConfig.Setup(x => x["Jwt:Key"]).Returns("super_secret_key_that_is_long_enough_for_HS512_super_secret_key_that_is_long_enough_for_HS512");
        _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("test_issuer");
        _mockConfig.Setup(x => x["Jwt:Audience"]).Returns("test_audience");
    }

    [Fact]
    public void GenerateTokenPair_ReturnsValidTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(user);

        // Assert
        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(accessToken);
        Assert.NotEmpty(refreshToken);
    }

    [Fact]
    public void GenerateTokenPair_AccessTokenContainsCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (accessToken, _) = _tokenService.GenerateTokenPair(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);

        // Assert
        Assert.Equal(user.Id.ToString(), jwtToken.Subject);
        Assert.Equal(user.Username, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.NotNull(jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    [Fact]
    public void GenerateTokenPair_AccessTokenHasCorrectIssuerAndAudience()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (accessToken, _) = _tokenService.GenerateTokenPair(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);

        // Assert
        Assert.Equal("test_issuer", jwtToken.Issuer);
        Assert.Equal("test_audience", jwtToken.Audiences.First());
    }

    [Fact]
    public void GenerateTokenPair_AccessTokenHasCorrectExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (accessToken, _) = _tokenService.GenerateTokenPair(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);

        // Assert
        var expectedExpiration = DateTime.UtcNow.AddMinutes(15);
        Assert.True(jwtToken.ValidTo <= expectedExpiration.AddMinutes(1));
        Assert.True(jwtToken.ValidTo >= expectedExpiration.AddMinutes(-1));
    }

    [Fact]
    public void GenerateTokenPair_RefreshTokenIsGuid()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (_, refreshToken) = _tokenService.GenerateTokenPair(user);

        // Assert
        Assert.True(Guid.TryParse(refreshToken, out _));
    }

    [Fact]
    public void GenerateTokenPair_ReturnsDifferentTokensForSameUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (accessToken1, refreshToken1) = _tokenService.GenerateTokenPair(user);
        var (accessToken2, refreshToken2) = _tokenService.GenerateTokenPair(user);

        // Assert
        Assert.NotEqual(accessToken1, accessToken2);
        Assert.NotEqual(refreshToken1, refreshToken2);
    }

    [Fact]
    public void GenerateTokenPair_UsesCorrectSecurityAlgorithm()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (accessToken, _) = _tokenService.GenerateTokenPair(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);

        // Assert
        // Ожидаем либо "HS512", либо полный URI алгоритма
        Assert.True(jwtToken.SignatureAlgorithm == SecurityAlgorithms.HmacSha512 ||
                    jwtToken.SignatureAlgorithm == "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512");
    }
}