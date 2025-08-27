using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.User;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // Только для авторизованных пользователей
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UserController(IUserService userService, IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var userProfile = await _userService.GetUserProfileAsync(userId);
            return Ok(userProfile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromForm] UpdateProfileRequestDto updateDto)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var updatedProfile = await _userService.UpdateUserProfileAsync(userId, updateDto);
            return Ok(updatedProfile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to update profile");
        }
    }
}