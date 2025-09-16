namespace TaskManagerAPI.Domain.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Order { get; set; }
    public bool DueDateNotificationSent { get; set; }

    public int TaskListId { get; set; }
    public TaskList TaskList { get; set; }
    public List<Label> Labels { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<Attachment> Attachments { get; set; } = new();
}