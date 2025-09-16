namespace TaskManagerAPI.Domain.Entities;

public class TaskList
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int BoardId { get; set; }
    public Board Board { get; set; }
    public List<Task> Tasks { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Order { get; set; }
}