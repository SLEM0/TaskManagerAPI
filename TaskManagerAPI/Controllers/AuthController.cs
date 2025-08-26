using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Claims;
using TaskManagerAPI.Application.Dtos.Auth;
using TaskManagerAPI.Application.Interfaces;

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
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Registration failed. Please try again.");
        }
    }

    [EnableRateLimiting("LoginLimiter")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest("Invalid email or password");
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException)
        {
            return StatusCode(503, "Database is unavailable. Please try again later.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("transient", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(503, "Database is initializing. Please try again in a moment.");
        }
        catch (Exception ex)
        {
            // Правильное логирование
            Debug.WriteLine($"Login error for email: {request.Email}. Error: {ex.Message}");
            return StatusCode(500, "Login failed. Please try again.");
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Token refresh failed. Please try again.");
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _authService.RevokeTokensAsync(userId);
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "Token revocation failed.");
        }
    }
}