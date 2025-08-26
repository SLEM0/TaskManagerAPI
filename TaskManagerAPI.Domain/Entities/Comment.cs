namespace TaskManagerAPI.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Связи
    public int TaskId { get; set; }
    public Task Task { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; }
}