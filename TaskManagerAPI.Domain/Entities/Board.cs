namespace TaskManagerAPI.Domain.Entities;

public class Board
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OwnerId { get; set; }
    public User Owner { get; set; }
    public List<TaskList> Lists { get; set; } = new();
    public List<Label> Labels { get; set; } = new();
    public List<Member> BoardUsers { get; set; } = new();
}