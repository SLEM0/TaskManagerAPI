using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Attachment;

namespace TaskManagerAPI.Application.Dtos.Task;

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskListId { get; set; }
    public int Order { get; set; }
    public IEnumerable<LabelResponseDto> Labels { get; set; }
    public IEnumerable<MemberResponseDto> Members { get; set; }
    public IEnumerable<CommentResponseDto> Comments { get; set; } 
    public IEnumerable<AttachmentResponseDto> Attachments { get; set; }
}