using TaskManagerAPI.Application.Dtos.Comment;

namespace TaskManagerAPI.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponseDto> AddCommentAsync(int taskId, CreateCommentRequestDto dto, int authorId);
    Task SystemLogActionAsync(int taskId, string action, int userId);
}