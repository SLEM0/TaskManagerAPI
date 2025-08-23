namespace TaskManagerAPI.Domain.Entities;

public class TaskList
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int BoardId { get; set; }  // ID доски
    public Board Board { get; set; }  // Доска
    public List<Task> Tasks { get; set; } = new();  // Задачи в списке
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Добавляем
    public int Order { get; set; }
}