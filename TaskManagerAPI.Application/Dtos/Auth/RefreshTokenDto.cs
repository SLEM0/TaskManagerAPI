using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Auth;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; }
}