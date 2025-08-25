namespace TaskManagerAPI.Domain.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Добавляем
    public int Order { get; set; }

    // Связи
    public int TaskListId { get; set; }  // ID списка
    public TaskList TaskList { get; set; }
    public List<Label> Labels { get; set; } = new();  // Метки задачи
    public List<BoardUser> Members { get; set; } = new(); // ← Используем BoardUser!
}