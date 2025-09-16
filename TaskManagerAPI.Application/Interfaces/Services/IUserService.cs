using TaskManagerAPI.Application.Dtos.User;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetUserProfileAsync(int userId);
    Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileRequestDto updateDto);
}