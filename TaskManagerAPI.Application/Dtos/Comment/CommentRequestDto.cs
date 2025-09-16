using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Comment;

public class CommentRequestDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; }
}