using AutoMapper;
using Moq;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICheckAccessService> _checkAccessServiceMock;
    private readonly IMapper _mapper;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _checkAccessServiceMock = new Mock<ICheckAccessService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Comment, CommentResponseDto>();
        });
        _mapper = config.CreateMapper();

        _commentService = new CommentService(
            _commentRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _userRepositoryMock.Object,
            _checkAccessServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_ValidRequest_ReturnsCommentResponse()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var authorId = 1;
        var dto = new CommentRequestDto { Content = "Test comment" };

        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };
        var author = new User { Id = authorId, Username = "testuser" };
        var comment = new Comment
        {
            Id = 1,
            Content = dto.Content,
            TaskId = taskId,
            AuthorId = authorId,
            Author = author
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((true, true));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _commentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => c.Id = 1);
        _commentRepositoryMock.Setup(x => x.GetByIdWithAuthorAsync(1))
            .ReturnsAsync(comment);

        // Act
        var result = await _commentService.AddCommentAsync(taskId, dto, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(comment.Id, result.Id);
        Assert.Equal(comment.Content, result.Content);
        _taskRepositoryMock.Verify(x => x.GetByIdWithTaskListAsync(taskId), Times.Once);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _commentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var taskId = 1;
        var authorId = 1;
        var dto = new CommentRequestDto { Content = "Test comment" };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync((Domain.Entities.Task)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _commentService.AddCommentAsync(taskId, dto, authorId));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var authorId = 1;
        var dto = new CommentRequestDto { Content = "Test comment" };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _commentService.AddCommentAsync(taskId, dto, authorId));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var authorId = 1;
        var dto = new CommentRequestDto { Content = "Test comment" };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((true, true));
        _userRepositoryMock.Setup(x => x.GetByIdAsync(authorId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _commentService.AddCommentAsync(taskId, dto, authorId));
    }

    [Fact]
    public async System.Threading.Tasks.Task SystemLogActionAsync_ValidRequest_CreatesSystemLog()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var action = "System action message";

        _commentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _commentService.SystemLogActionAsync(taskId, action, userId);

        // Assert
        _commentRepositoryMock.Verify(x => x.AddAsync(It.Is<Comment>(c =>
            c.TaskId == taskId &&
            c.AuthorId == userId &&
            c.Content == action &&
            c.IsSystemLog == true
        )), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task SystemLogActionAsync_WithNullAction_CreatesSystemLogWithEmptyContent()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        string action = null;

        _commentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _commentService.SystemLogActionAsync(taskId, action, userId);

        // Assert
        _commentRepositoryMock.Verify(x => x.AddAsync(It.Is<Comment>(c =>
            c.TaskId == taskId &&
            c.AuthorId == userId &&
            c.Content == null &&
            c.IsSystemLog == true
        )), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task SystemLogActionAsync_WithEmptyAction_CreatesSystemLogWithEmptyContent()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var action = "";

        _commentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _commentService.SystemLogActionAsync(taskId, action, userId);

        // Assert
        _commentRepositoryMock.Verify(x => x.AddAsync(It.Is<Comment>(c =>
            c.TaskId == taskId &&
            c.AuthorId == userId &&
            c.Content == "" &&
            c.IsSystemLog == true
        )), Times.Once);
    }
}