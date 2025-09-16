using AutoMapper;
using Moq;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class TaskListServiceTests
{
    private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
    private readonly Mock<ICheckAccessService> _authServiceMock;
    private readonly IMapper _mapper;
    private readonly TaskListService _taskListService;

    public TaskListServiceTests()
    {
        _taskListRepositoryMock = new Mock<ITaskListRepository>();
        _authServiceMock = new Mock<ICheckAccessService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TaskList, TaskListResponseDto>();
        });
        _mapper = config.CreateMapper();

        _taskListService = new TaskListService(
            _taskListRepositoryMock.Object,
            _authServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskListDetailsAsync_ValidRequest_ReturnsTaskListDetails()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var taskList = new TaskList
        {
            Id = taskListId,
            Title = "Test List",
            BoardId = boardId
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(taskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((true, true));

        // Act
        var result = await _taskListService.GetTaskListDetailsAsync(taskListId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskList.Id, result.Id);
        Assert.Equal(taskList.Title, result.Title);
        _taskListRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskListId), Times.Once);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskListDetailsAsync_TaskListNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var taskListId = 1;

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync((TaskList)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _taskListService.GetTaskListDetailsAsync(taskListId));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskListDetailsAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var taskList = new TaskList
        {
            Id = taskListId,
            BoardId = boardId
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(taskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _taskListService.GetTaskListDetailsAsync(taskListId));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskListAsync_ValidRequest_ReturnsCreatedTaskList()
    {
        // Arrange
        var boardId = 1;
        var taskListDto = new TaskListRequestDto { Title = "New List" };
        var taskList = new TaskList
        {
            Id = 1,
            Title = taskListDto.Title,
            BoardId = boardId,
            Order = 1
        };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskListRepositoryMock.Setup(x => x.GetMaxOrderAsync(boardId))
            .ReturnsAsync(0);
        _taskListRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskList>()))
            .Callback<TaskList>(tl => tl.Id = 1);
        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(taskList);

        // Act
        var result = await _taskListService.CreateTaskListAsync(taskListDto, boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskList.Id, result.Id);
        Assert.Equal(taskList.Title, result.Title);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskListRepositoryMock.Verify(x => x.GetMaxOrderAsync(boardId), Times.Once);
        _taskListRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskList>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskListAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var boardId = 1;
        var taskListDto = new TaskListRequestDto { Title = "New List" };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _taskListService.CreateTaskListAsync(taskListDto, boardId));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskListAsync_ValidRequest_ReturnsUpdatedTaskList()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var taskListDto = new TaskListRequestDto { Title = "Updated List" };
        var existingTaskList = new TaskList
        {
            Id = taskListId,
            Title = "Old List",
            BoardId = boardId
        };
        var updatedTaskList = new TaskList
        {
            Id = taskListId,
            Title = taskListDto.Title,
            BoardId = boardId
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(existingTaskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskListRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskList>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(updatedTaskList);

        // Act
        var result = await _taskListService.UpdateTaskListAsync(taskListId, taskListDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskListDto.Title, result.Title);
        _taskListRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskListId), Times.AtLeastOnce);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskListRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskList>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskListAsync_ValidRequest_DeletesTaskList()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var taskList = new TaskList
        {
            Id = taskListId,
            BoardId = boardId
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(taskListId))
            .ReturnsAsync(taskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskListRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<TaskList>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _taskListService.DeleteTaskListAsync(taskListId);

        // Assert
        _taskListRepositoryMock.Verify(x => x.GetByIdWithBoardAsync(taskListId), Times.Once);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskListRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TaskList>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveTaskListAsync_ValidRequest_MovesTaskList()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var moveDto = new MoveTaskListRequestDto { NewOrder = 1 };
        var taskList = new TaskList
        {
            Id = taskListId,
            BoardId = boardId,
            Order = 0
        };
        var otherLists = new List<TaskList>
        {
            new TaskList { Id = 2, BoardId = boardId, Order = 1 },
            new TaskList { Id = 3, BoardId = boardId, Order = 2 }
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(taskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskListRepositoryMock.Setup(x => x.GetByBoardIdAsync(boardId))
            .ReturnsAsync(otherLists);
        _taskListRepositoryMock.Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<TaskList>>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(taskList);

        // Act
        var result = await _taskListService.MoveTaskListAsync(taskListId, moveDto);

        // Assert
        Assert.NotNull(result);
        _taskListRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(taskListId), Times.AtLeastOnce);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _taskListRepositoryMock.Verify(x => x.GetByBoardIdAsync(boardId), Times.Once);
        _taskListRepositoryMock.Verify(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<TaskList>>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveTaskListAsync_InvalidOrder_ThrowsValidationException()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var moveDto = new MoveTaskListRequestDto { NewOrder = 10 };
        var taskList = new TaskList
        {
            Id = taskListId,
            BoardId = boardId
        };
        var otherLists = new List<TaskList>
        {
            new TaskList { Id = 2, BoardId = boardId, Order = 1 }
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(taskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _taskListRepositoryMock.Setup(x => x.GetByBoardIdAsync(boardId))
            .ReturnsAsync(otherLists);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _taskListService.MoveTaskListAsync(taskListId, moveDto));
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveTaskListAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var taskListId = 1;
        var boardId = 1;
        var moveDto = new MoveTaskListRequestDto { NewOrder = 1 };
        var taskList = new TaskList
        {
            Id = taskListId,
            BoardId = boardId
        };

        _taskListRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(taskListId))
            .ReturnsAsync(taskList);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _taskListService.MoveTaskListAsync(taskListId, moveDto));
    }
}