using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Domain.Entities;

public class Member
{
    public int Id { get; set; }

    public int BoardId { get; set; }
    public Board Board { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public BoardRole Role { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}