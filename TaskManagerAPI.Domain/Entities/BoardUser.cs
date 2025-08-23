using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Domain.Entities;

public class BoardUser
{
    public int Id { get; set; }

    // Связи
    public int BoardId { get; set; }
    public Board Board { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public BoardRole Role { get; set; }  // Вместо string

    public DateTime AddedAt { get; set; } = DateTime.UtcNow; // Новое свойство
}