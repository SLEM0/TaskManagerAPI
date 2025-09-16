using AutoMapper;
using Moq;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
    private readonly Mock<ILabelRepository> _labelRepositoryMock;
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly Mock<ICheckAccessService> _checkAccessServiceMock;
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly IMapper _mapper;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskListRepositoryMock = new Mock<ITaskListRepository>();
        _labelRepositoryMock = new Mock<ILabelRepository>();
        _memberRepositoryMock = new Mock<IMemberRepository>();
        _checkAccessServiceMock = new Mock<ICheckAccessService>();
        _commentServiceMock = new Mock<ICommentService>();
        _userContextMock = new Mock<IUserContext>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Domain.Entities.Task, TaskResponseDto>()
                .ForMember(dest => dest.Labels, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            cfg.CreateMap<TaskRequestDto, Domain.Entities.Task>()
                .ForMember(dest => dest.Labels, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TaskList, opt => opt.Ignore());

            cfg.CreateMap<Label, LabelResponseDto>();
            cfg.CreateMap<Member, MemberResponseDto>();

            cfg.CreateMap<User, object>().ReverseMap();
            cfg.CreateMap<Comment, object>().ReverseMap();
            cfg.CreateMap<Attachment, object>().ReverseMap();
            cfg.CreateMap<TaskList, object>().ReverseMap();
        });
        _mapper = config.CreateMapper();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _taskListRepositoryMock.Object,
            _labelRepositoryMock.Object,
            _memberRepositoryMock.Object,
            _checkAccessServiceMock.Object,
            _commentServiceMock.Object,
            _userContextMock.Object,
            _mapper);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ValidRequest_ReturnsTaskResponse()
    {
        // Arrange
        var listId = 1;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var taskDto = new TaskRequestDto
        {
            Title = "Test Task",
            Description = "Test Description"
        };
        var taskList = new TaskList { Id = listId, BoardId = boardId };
        var task = new Domain.Entities.Task
        {
            Id = 1,
            Title = taskDto.Title,
            Description = taskDto.Description,
            TaskListId = listId,
            TaskList = taskList
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(listId))
            .ReturnsAsync(taskList);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskRepositoryMock.Setup(x => x.GetMaxOrderAsync(listId))
            .ReturnsAsync(0);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Task>()))
            .Callback<Domain.Entities.Task>(t => t.Id = 1);
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.CreateTaskAsync(taskDto, listId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
        _taskListRepositoryMock.Verify(x => x.GetByIdWithBoardAsync(listId), Times.Once);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(1, It.IsAny<string>(), userId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_TaskListNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var listId = 1;
        var taskDto = new TaskRequestDto { Title = "Test Task" };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(listId))
            .ReturnsAsync((TaskList)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _taskService.CreateTaskAsync(taskDto, listId));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var listId = 1;
        var boardId = 1;
        var taskDto = new TaskRequestDto { Title = "Test Task" };
        var taskList = new TaskList { Id = listId, BoardId = boardId };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(listId))
            .ReturnsAsync(taskList);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _taskService.CreateTaskAsync(taskDto, listId));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskDetailsAsync_ValidRequest_ReturnsTaskDetails()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Title = "Test Task",
            TaskList = new TaskList { BoardId = boardId }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((true, true));

        // Act
        var result = await _taskService.GetTaskDetailsAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.Once);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_ValidRequest_UpdatesTask()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var taskDto = new TaskRequestDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            IsCompleted = true
        };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Title = "Old Title",
            Description = "Old Description",
            IsCompleted = false,
            TaskList = new TaskList { BoardId = boardId }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, taskDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.Title, result.Title);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.AtLeastOnce);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), userId), Times.AtLeastOnce);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskAsync_ValidRequest_DeletesTask()
    {
        // Arrange
        var taskId = 1;
        var boardId = 1;
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskList = new TaskList { BoardId = boardId }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithTaskListAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _taskService.DeleteTaskAsync(taskId);

        // Assert
        _taskRepositoryMock.Verify(x => x.GetByIdWithTaskListAsync(taskId), Times.Once);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveTaskAsync_ValidRequest_MovesTask()
    {
        // Arrange
        var taskId = 1;
        var sourceListId = 1;
        var targetListId = 2;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var moveDto = new MoveTaskRequestDto { NewListId = targetListId, NewOrder = 0 };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            TaskListId = sourceListId,
            TaskList = new TaskList { Id = sourceListId, BoardId = boardId }
        };
        var sourceList = new TaskList { Id = sourceListId, BoardId = boardId };
        var targetList = new TaskList { Id = targetListId, BoardId = boardId, Title = "Target List" };
        var tasksInTargetList = new List<Domain.Entities.Task>();

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _taskListRepositoryMock.Setup(x => x.GetByIdAsync(sourceListId))
            .ReturnsAsync(sourceList);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskListRepositoryMock.Setup(x => x.GetByIdAsync(targetListId))
            .ReturnsAsync(targetList);
        _taskRepositoryMock.Setup(x => x.GetByTaskListIdAsync(targetListId))
            .ReturnsAsync(tasksInTargetList);
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _taskRepositoryMock.Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<Domain.Entities.Task>>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.MoveTaskAsync(taskId, moveDto);

        // Assert
        Assert.NotNull(result);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.AtLeastOnce);
        _checkAccessServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.AtLeastOnce);
        _taskRepositoryMock.Verify(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<Domain.Entities.Task>>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddLabelToTaskAsync_ValidRequest_AddsLabel()
    {
        // Arrange
        var taskId = 1;
        var labelId = 1;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var taskList = new TaskList { Id = 1, BoardId = boardId };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Labels = new List<Label>(),
            TaskList = taskList
        };
        var label = new Label { Id = labelId, Name = "Test Label" };

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _labelRepositoryMock.Setup(x => x.GetByIdAsync(labelId))
            .ReturnsAsync(label);
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.AddLabelToTaskAsync(taskId, labelId);

        // Assert
        Assert.NotNull(result);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.AtLeastOnce);
        _labelRepositoryMock.Verify(x => x.GetByIdAsync(labelId), Times.Once);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), userId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveLabelFromTaskAsync_ValidRequest_RemovesLabel()
    {
        // Arrange
        var taskId = 1;
        var labelId = 1;
        var boardId = 1;
        var userId = 1;
        var userName = "testuser";
        var taskList = new TaskList { Id = 1, BoardId = boardId };
        var label = new Label { Id = labelId, Name = "Test Label" };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Labels = new List<Label> { label },
            TaskList = taskList
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.RemoveLabelFromTaskAsync(taskId, labelId);

        // Assert
        Assert.NotNull(result);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.AtLeastOnce);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), userId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AssignTaskAsync_ValidRequest_AssignsUser()
    {
        // Arrange
        var taskId = 1;
        var userId = 2;
        var boardId = 1;
        var currentUserId = 1;
        var userName = "testuser";
        var taskList = new TaskList { Id = 1, BoardId = boardId };
        var member = new Member
        {
            UserId = userId,
            User = new User { Id = userId, Username = "assigneduser", Email = "test@test.com" }
        };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Members = new List<Member>(),
            TaskListId = 1,
            TaskList = taskList
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _taskListRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(taskList);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _memberRepositoryMock.Setup(x => x.GetByBoardAndUserIdAsync(boardId, userId))
            .ReturnsAsync(member);
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(currentUserId);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, userId);

        // Assert
        Assert.NotNull(result);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.AtLeastOnce);
        _memberRepositoryMock.Verify(x => x.GetByBoardAndUserIdAsync(boardId, userId), Times.Once);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), currentUserId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignTaskAsync_ValidRequest_UnassignsUser()
    {
        // Arrange
        var taskId = 1;
        var userId = 2;
        var boardId = 1;
        var currentUserId = 1;
        var userName = "testuser";
        var taskList = new TaskList { Id = 1, BoardId = boardId };
        var member = new Member
        {
            UserId = userId,
            User = new User { Id = userId, Username = "assigneduser", Email = "test@test.com" }
        };
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Members = new List<Member> { member },
            TaskListId = 1,
            TaskList = taskList
        };

        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);
        _taskListRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(taskList);
        _checkAccessServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _userContextMock.Setup(x => x.GetCurrentUserName())
            .Returns(userName);
        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(currentUserId);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.UnassignTaskAsync(taskId, userId);

        // Assert
        Assert.NotNull(result);
        _taskRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskId), Times.AtLeastOnce);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
        _commentServiceMock.Verify(x => x.SystemLogActionAsync(taskId, It.IsAny<string>(), currentUserId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTasksDueBetweenAsync_ValidRequest_ReturnsTasks()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddDays(7);
        var tasks = new List<Domain.Entities.Task>
        {
            new Domain.Entities.Task { Id = 1, Title = "Task 1", DueDate = start.AddDays(1) },
            new Domain.Entities.Task { Id = 2, Title = "Task 2", DueDate = start.AddDays(3) }
        };

        _taskRepositoryMock.Setup(x => x.GetTasksDueBetweenAsync(start, end))
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetTasksDueBetweenAsync(start, end);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _taskRepositoryMock.Verify(x => x.GetTasksDueBetweenAsync(start, end), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task MarkDueDateNotificationSentAsync_ValidRequest_MarksNotificationSent()
    {
        // Arrange
        var taskId = 1;
        var task = new Domain.Entities.Task { Id = taskId, DueDateNotificationSent = false };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _taskService.MarkDueDateNotificationSentAsync(taskId);

        // Assert
        Assert.True(task.DueDateNotificationSent);
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(taskId), Times.Once);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
    }
}