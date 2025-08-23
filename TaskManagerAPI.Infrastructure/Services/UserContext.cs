using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Infrastructure.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _cachedUserId;

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
            throw new UnauthorizedAccessException("Invalid user ID in token");

        _cachedUserId = userId;
        return userId;
    }
}