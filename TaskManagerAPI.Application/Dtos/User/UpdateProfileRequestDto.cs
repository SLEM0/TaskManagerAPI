using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.User;

public class UpdateProfileRequestDto
{
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
    public string Username { get; set; }

    public IFormFile AvatarFile { get; set; } // ← Файл аватара вместо URL

    public bool RemoveAvatar { get; set; } // ← Флаг для сброса на дефолтный
}