using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskManagerAPI.Application.Dtos.Auth;
using TaskManagerAPI.Application.Interfaces.Services;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register(UserRegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [EnableRateLimiting("LoginLimiter")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        await _authService.RevokeTokensAsync(userId);
        return NoContent();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto request)
    {
        var result = await _authService.ConfirmEmailAsync(request.Code);
        return Ok(result);
    }
}