//using Microsoft.EntityFrameworkCore;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Domain.Entities;
//using TaskManagerAPI.Domain.Enums;
//using TaskManagerAPI.Dtos.Task;
//using TaskManagerAPI.Infrastructure.Data;
//using TaskManagerAPI.Infrastructure.Services;

//namespace TaskManagerAPI.Tests.Services;

//public class TaskServiceTests : IDisposable
//{
//    private readonly DbContextOptions<AppDbContext> _dbOptions;
//    private readonly Mock<IAuthorizationService> _mockAuthService;
//    private readonly TaskService _taskService;

//    public TaskServiceTests()
//    {
//        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _mockAuthService = new Mock<IAuthorizationService>();
//        _taskService = new TaskService(new AppDbContext(_dbOptions), _mockAuthService.Object);
//    }

//    public void Dispose()
//    {
//        using var context = new AppDbContext(_dbOptions);
//        context.Database.EnsureDeleted();
//    }

//    private Board CreateTestBoard(int id, int ownerId)
//    {
//        return new Board
//        {
//            Id = id,
//            Title = $"Test Board {id}",
//            Description = $"Description {id}",
//            OwnerId = ownerId,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    private TaskList CreateTestTaskList(int id, int boardId)
//    {
//        return new TaskList
//        {
//            Id = id,
//            Title = $"Test List {id}",
//            BoardId = boardId,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    private Domain.Entities.Task CreateTestTask(int id, int listId, string title = "Test Task")
//    {
//        return new Models.Task
//        {
//            Id = id,
//            Title = title,
//            Description = $"Description for {title}",
//            TaskListId = listId,
//            IsCompleted = false,
//            Order = 1,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    private Label CreateTestLabel(int id, int boardId)
//    {
//        return new Label
//        {
//            Id = id,
//            Name = $"Label {id}",
//            Color = "#FF0000",
//            BoardId = boardId,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateTaskAsync_WhenAuthorized_CreatesTask()
//    {
//        // Arrange
//        var listId = 1;
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskDto
//        {
//            Title = "New Task",
//            Description = "Task Description",
//            DueDate = DateTime.UtcNow.AddDays(7)
//        };

//        _mockAuthService.Setup(x => x.CheckTaskListAccessAsync(listId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            board.Lists = new List<TaskList> { taskList };

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskService.CreateTaskAsync(createDto, listId, userId);

//        // Assert
//        Assert.Equal("New Task", result.Title);
//        Assert.Equal("Task Description", result.Description);
//        Assert.Equal(1, result.Order);

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var task = await context.Tasks.FirstOrDefaultAsync();
//            Assert.NotNull(task);
//            Assert.Equal(listId, task.TaskListId);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetTaskDetailsAsync_WhenAuthorized_ReturnsTaskDetails()
//    {
//        // Arrange
//        var taskId = 1;
//        var listId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var task = CreateTestTask(taskId, listId);

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskService.GetTaskDetailsAsync(taskId, userId);

//        // Assert
//        Assert.Equal(taskId, result.Id);
//        Assert.Equal("Test Task", result.Title);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task UpdateTaskAsync_WhenAuthorized_UpdatesTask()
//    {
//        // Arrange
//        var taskId = 1;
//        var listId = 1;
//        var boardId = 1;
//        var userId = 1;
//        var updateDto = new UpdateTaskDto
//        {
//            Title = "Updated Task",
//            Description = "Updated Description",
//            DueDate = DateTime.UtcNow.AddDays(14)
//        };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var task = CreateTestTask(taskId, listId);

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskService.UpdateTaskAsync(taskId, updateDto, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var task = await context.Tasks.FindAsync(taskId);
//            Assert.Equal("Updated Task", task.Title);
//            Assert.Equal("Updated Description", task.Description);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task DeleteTaskAsync_WhenAuthorized_DeletesTask()
//    {
//        // Arrange
//        var taskId = 1;
//        var listId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var task = CreateTestTask(taskId, listId);

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskService.DeleteTaskAsync(taskId, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var task = await context.Tasks.FindAsync(taskId);
//            Assert.Null(task);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task ToggleTaskCompletionAsync_WhenAuthorized_TogglesCompletion()
//    {
//        // Arrange
//        var taskId = 1;
//        var listId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var task = CreateTestTask(taskId, listId);

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskService.ToggleTaskCompletionAsync(taskId, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var task = await context.Tasks.FindAsync(taskId);
//            Assert.True(task.IsCompleted);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task AddLabelToTaskAsync_WhenAuthorized_AddsLabel()
//    {
//        // Arrange
//        var taskId = 1;
//        var listId = 1;
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;
//        var addLabelDto = new AddLabelToTaskDto { LabelId = labelId };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var task = CreateTestTask(taskId, listId);
//            var label = CreateTestLabel(labelId, boardId);

//            board.Lists = new List<TaskList> { taskList };
//            board.Labels = new List<Label> { label };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskService.AddLabelToTaskAsync(taskId, addLabelDto, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var task = await context.Tasks
//                .Include(t => t.Labels)
//                .FirstOrDefaultAsync(t => t.Id == taskId);
//            Assert.Single(task.Labels);
//            Assert.Equal(labelId, task.Labels.First().Id);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task RemoveLabelFromTaskAsync_WhenAuthorized_RemovesLabel()
//    {
//        // Arrange
//        var taskId = 1;
//        var listId = 1;
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var label = CreateTestLabel(labelId, boardId);
//            var task = CreateTestTask(taskId, listId);
//            task.Labels = new List<Label> { label };

//            board.Lists = new List<TaskList> { taskList };
//            board.Labels = new List<Label> { label };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskService.RemoveLabelFromTaskAsync(taskId, labelId, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var task = await context.Tasks
//                .Include(t => t.Labels)
//                .FirstOrDefaultAsync(t => t.Id == taskId);
//            Assert.Empty(task.Labels);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetFilteredTasksAsync_ReturnsFilteredTasks()
//    {
//        // Arrange
//        var userId = 1;
//        var boardId = 1;
//        var listId = 1;

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, userId);
//            var taskList = CreateTestTaskList(listId, boardId);
//            var task = CreateTestTask(1, listId);
//            task.DueDate = DateTime.UtcNow.AddDays(1);

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskService.GetFilteredTasksAsync(
//            DateTime.UtcNow.AddDays(1).Date, null, 1, 20, userId);

//        // Assert
//        var taskDto = Assert.Single(result);
//        Assert.Equal("Test Task", taskDto.Title);
//    }
//}