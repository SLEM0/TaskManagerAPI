using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Services;

namespace TaskManagerAPI.Infrastructure.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _cachedUserId;
    private string? _cachedUserName;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentUserId()
    {
        if (_cachedUserId.HasValue)
            return _cachedUserId.Value;

        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value) || !int.TryParse(userIdClaim.Value, out var userId))
            throw new ForbiddenAccessException("Invalid user ID in token");

        _cachedUserId = userId;
        return userId;
    }

    public string GetCurrentUserName()
    {
        if (_cachedUserName != null)
            return _cachedUserName;

        var usernameClaim = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Name);

        if (usernameClaim == null || string.IsNullOrEmpty(usernameClaim.Value))
            throw new ForbiddenAccessException("Invalid username in token");

        _cachedUserName = usernameClaim.Value;
        return _cachedUserName;
    }
}