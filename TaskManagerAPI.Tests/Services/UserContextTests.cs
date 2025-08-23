using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class UserContextTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly UserContext _userContext;

    public UserContextTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _userContext = new UserContext(_mockHttpContextAccessor.Object);
    }

    [Fact]
    public void GetCurrentUserId_WithValidClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = 1;
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
            }));

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var userId = _userContext.GetCurrentUserId();

        // Assert
        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void GetCurrentUserId_WithCachedValue_ReturnsCachedUserId()
    {
        // Arrange
        var expectedUserId = 1;
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
            }));

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Первый вызов - заполняем кеш
        _userContext.GetCurrentUserId();

        // Изменяем контекст, но кеш должен сохраниться
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

        // Act
        var userId = _userContext.GetCurrentUserId();

        // Assert
        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void GetCurrentUserId_WithMissingClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => _userContext.GetCurrentUserId());
    }

    [Fact]
    public void GetCurrentUserId_WithInvalidClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
                new Claim(ClaimTypes.NameIdentifier, "not_an_integer")
            }));

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => _userContext.GetCurrentUserId());
    }
}