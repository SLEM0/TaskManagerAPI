using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskManagerAPI.Application.Dtos.User;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;

namespace TaskManagerAPI.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly string _webRootPath;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<UserProfileDto> GetUserProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User not found");

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileRequestDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User not found");

        if (!string.IsNullOrEmpty(updateDto.Username))
        {
            if (user.Username != updateDto.Username)
            {
                var usernameExists = await _userRepository.UsernameExistsAsync(updateDto.Username, userId);
                if (usernameExists)
                    throw new ValidationException("Username already taken");

                user.Username = updateDto.Username;
            }
        }

        if (updateDto.RemoveAvatar)
        {
            user.AvatarUrl = "/avatars/default-avatar.png";
        }
        else if (updateDto.AvatarFile != null && updateDto.AvatarFile.Length > 0)
        {
            user.AvatarUrl = await UploadAvatarFileAsync(updateDto.AvatarFile, userId);
        }

        await _userRepository.UpdateAsync(user);

        return _mapper.Map<UserProfileDto>(user);
    }

    private async Task<string> UploadAvatarFileAsync(IFormFile file, int userId)
    {
        if (file.Length > 5 * 1024 * 1024)
            throw new ValidationException("File too large (max 5MB)");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ValidationException("Invalid file type. Allowed: JPG, JPEG, PNG, GIF");

        var uploadsFolder = Path.Combine(_webRootPath, "avatars");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"avatar_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/avatars/{fileName}";
    }
}