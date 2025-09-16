using AutoMapper;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICheckAccessService _checkAccessService;
    private readonly IMapper _mapper;

    public CommentService(
        ICommentRepository commentRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        ICheckAccessService checkAccessService,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _checkAccessService = checkAccessService;
        _mapper = mapper;
    }

    public async Task<CommentResponseDto> AddCommentAsync(int taskId, CommentRequestDto dto, int authorId)
    {
        var task = await _taskRepository.GetByIdWithTaskListAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new ForbiddenAccessException();

        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null) throw new NotFoundException("User not found");

        var comment = new Comment
        {
            Content = dto.Content,
            TaskId = taskId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow,
            IsSystemLog = false
        };

        await _commentRepository.AddAsync(comment);

        var createdComment = await _commentRepository.GetByIdWithAuthorAsync(comment.Id);
        if (createdComment == null) throw new NotFoundException("Comment not found after creation");

        return _mapper.Map<CommentResponseDto>(createdComment);
    }

    public async System.Threading.Tasks.Task SystemLogActionAsync(int taskId, string action, int userId)
    {
        var comment = new Comment
        {
            Content = action,
            TaskId = taskId,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow,
            IsSystemLog = true
        };

        await _commentRepository.AddAsync(comment);
    }
}