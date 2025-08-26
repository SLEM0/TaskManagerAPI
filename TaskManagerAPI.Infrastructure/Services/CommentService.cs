using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _checkAccessService;

    public CommentService(AppDbContext context, ICheckAccessService checkAccessService)
    {
        _context = context;
        _checkAccessService = checkAccessService;
    }

    public async Task<CommentResponseDto> AddCommentAsync(int taskId, CreateCommentRequestDto dto, int authorId)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var author = await _context.Users.FindAsync(authorId);
        if (author == null) throw new KeyNotFoundException("User not found");

        var comment = new Comment
        {
            Content = dto.Content,
            TaskId = taskId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentResponseDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = author.Username
        };
    }

    public async System.Threading.Tasks.Task SystemLogActionAsync(int taskId, string action, int userId)
    {
        var comment = new Comment
        {
            Content = $"[System] {action}",
            TaskId = taskId,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }
}