using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class AttachmentServiceTests
{
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ICheckAccessService> _checkAccessServiceMock;
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly IMapper _mapper;
    private readonly AttachmentService _attachmentService;

    public AttachmentServiceTests()
    {
        _attachmentRepositoryMock = new Mock<IAttachmentRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _checkAccessServiceMock = new Mock<ICheckAccessService>();
        _commentServiceMock = new Mock<ICommentService>();
        _userContextMock = new Mock<IUserContext>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Attachment, AttachmentResponseDto>();
        });
        _mapper = config.CreateMapper();

        _attachmentService = new AttachmentService(
            _attachmentRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _checkAccessServiceMock.Object,
            _commentServiceMock.Object,
            _userContextMock.Object,
            _mapper);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAttachmentAsync_ValidFile_ReturnsAttachmentResponse()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var fileName = "test.txt";
        var fileContent = "test content";

        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(fileContent.Length);
        fileMock.Setup(f => f.ContentType).Returns("text/plain");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var attachment = new Attachment
        {
            Id = 1,
            FileName = fileName,
            TaskId = taskId,
            UploadedById = userId
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _attachmentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Attachment>()))
            .Callback<Attachment>(a => a.Id = 1);
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _attachmentRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(attachment);

        // Act
        var result = await _attachmentService.AddAttachmentAsync(taskId, fileMock.Object, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(attachment.Id, result.Id);
        Assert.Equal(attachment.FileName, result.FileName);
        _taskRepositoryMock.Verify(x => x.GetByIdWithTaskListAsync(taskId), Times.Once);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _attachmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Attachment>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), userId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAttachmentAsync_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var fileMock = new Mock<IFormFile>();

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync((Domain.Entities.Task)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _attachmentService.AddAttachmentAsync(taskId, fileMock.Object, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAttachmentAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var userId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };
        var fileMock = new Mock<IFormFile>();

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _attachmentService.AddAttachmentAsync(taskId, fileMock.Object, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAttachmentAsync_InvalidFileSize_ThrowsValidationException()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var userId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(11 * 1024 * 1024);
        fileMock.Setup(f => f.FileName).Returns("test.txt");

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _attachmentService.AddAttachmentAsync(taskId, fileMock.Object, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAttachmentAsync_InvalidFileType_ThrowsValidationException()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var userId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.exe");

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _attachmentService.AddAttachmentAsync(taskId, fileMock.Object, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveAttachmentAsync_ValidRequest_RemovesAttachment()
    {
        // Arrange
        var taskId = 1;
        var attachmentId = 1;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };
        var attachment = new Attachment
        {
            Id = attachmentId,
            TaskId = taskId,
            FileName = "test.txt",
            FilePath = "test-path.txt"
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _attachmentRepositoryMock.Setup(x => x.GetByIdAsync(attachmentId))
            .ReturnsAsync(attachment);
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _attachmentRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Attachment>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _attachmentService.RemoveAttachmentAsync(taskId, attachmentId, userId);

        // Assert
        _taskRepositoryMock.Verify(x => x.GetByIdWithTaskListAsync(taskId), Times.Once);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _attachmentRepositoryMock.Verify(x => x.GetByIdAsync(attachmentId), Times.Once);
        _attachmentRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Attachment>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), userId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveAttachmentAsync_AttachmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var taskId = 1;
        var attachmentId = 1;
        var boardId = 1;
        var userId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _attachmentRepositoryMock.Setup(x => x.GetByIdAsync(attachmentId))
            .ReturnsAsync((Attachment)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _attachmentService.RemoveAttachmentAsync(taskId, attachmentId, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveAttachmentAsync_AttachmentWrongTask_ThrowsNotFoundException()
    {
        // Arrange
        var taskId = 1;
        var attachmentId = 1;
        var boardId = 1;
        var userId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };
        var attachment = new Attachment
        {
            Id = attachmentId,
            TaskId = 999
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _attachmentRepositoryMock.Setup(x => x.GetByIdAsync(attachmentId))
            .ReturnsAsync(attachment);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _attachmentService.RemoveAttachmentAsync(taskId, attachmentId, userId));
    }
}