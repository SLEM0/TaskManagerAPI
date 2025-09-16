using TaskManagerAPI.Application.Dtos.Comment;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface ICommentService
{
    Task<CommentResponseDto> AddCommentAsync(int taskId, CommentRequestDto dto, int authorId);
    Task SystemLogActionAsync(int taskId, string action, int userId);
}