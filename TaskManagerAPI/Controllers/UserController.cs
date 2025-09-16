using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.User;
using TaskManagerAPI.Application.Interfaces.Services;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] 
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
        var userId = _userContext.GetCurrentUserId();
        var userProfile = await _userService.GetUserProfileAsync(userId);
        return Ok(userProfile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromForm] UpdateProfileRequestDto updateDto)
    {
        var userId = _userContext.GetCurrentUserId();
        var updatedProfile = await _userService.UpdateUserProfileAsync(userId, updateDto);
        return Ok(updatedProfile);
    }
}