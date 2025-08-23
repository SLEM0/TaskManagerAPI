using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Member;

public class RemoveMemberRequestDto
{
    [Required]
    public int UserId { get; set; }
}