using System.ComponentModel.DataAnnotations;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Application.Dtos.Member;

public class MemberRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } // Меняем UserId на Email!

    [Required]
    [RegularExpression("Editor|Viewer", ErrorMessage = "Role must be either Editor or Viewer")]
    public BoardRole Role { get; set; }
}