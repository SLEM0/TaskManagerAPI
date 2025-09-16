namespace TaskManagerAPI.Application.Dtos.Comment;

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; }
    public bool IsSystemLog { get; set; }
}