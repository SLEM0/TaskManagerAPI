using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Application.Dtos.Member;

public class MemberResponseDto
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } // Добавил для отображения имени
    public string UserEmail { get; set; } // Добавил для отображения email
    public BoardRole Role { get; set; }
    public DateTime AddedAt { get; set; }
}