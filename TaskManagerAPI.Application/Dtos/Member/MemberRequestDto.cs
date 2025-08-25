using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Member;

public class MemberRequestDto
{
    [Required]
    public int UserId { get; set; }
}