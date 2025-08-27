using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TaskManagerAPI.Application.Dtos.User;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly string _webRootPath;

    public UserService(AppDbContext context)
    {
        _context = context;
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<UserProfileDto> GetUserProfileAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new KeyNotFoundException("User not found");

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.AvatarUrl
        };
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileRequestDto updateDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new KeyNotFoundException("User not found");

        // Обновляем username если передан
        if (!string.IsNullOrEmpty(updateDto.Username))
        {
            if (user.Username != updateDto.Username)
            {
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.Username == updateDto.Username && u.Id != userId);

                if (usernameExists)
                    throw new InvalidOperationException("Username already taken");

                user.Username = updateDto.Username;
            }
        }

        // Обрабатываем аватар
        if (updateDto.RemoveAvatar)
        {
            // Сброс на дефолтный аватар
            user.AvatarUrl = "/avatars/default-avatar.png";
        }
        else if (updateDto.AvatarFile != null && updateDto.AvatarFile.Length > 0)
        {
            // Загрузка нового аватара
            user.AvatarUrl = await UploadAvatarFileAsync(updateDto.AvatarFile, userId);
        }

        await _context.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<string> UploadAvatarFileAsync(IFormFile file, int userId)
    {
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File too large (max 5MB)");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException("Invalid file type. Allowed: JPG, JPEG, PNG, GIF");

        // Используем вычисленный путь
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