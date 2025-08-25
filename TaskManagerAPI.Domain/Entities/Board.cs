namespace TaskManagerAPI.Domain.Entities;

public class Board
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Связи
    public int OwnerId { get; set; }  // ID создателя
    public User Owner { get; set; }  // Владелец доски
    public List<TaskList> Lists { get; set; } = new();  // Списки задач в доске
    public List<Label> Labels { get; set; } = new();

    // Добавляем связь с участниками
    public List<BoardUser> BoardUsers { get; set; } = new();
}