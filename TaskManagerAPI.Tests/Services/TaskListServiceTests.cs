//using Microsoft.EntityFrameworkCore;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Domain.Entities;
//using TaskManagerAPI.Domain.Enums;
//using TaskManagerAPI.Dtos.TaskList;
//using TaskManagerAPI.Infrastructure.Data;
//using TaskManagerAPI.Infrastructure.Services;

//namespace TaskManagerAPI.Tests.Services;

//public class TaskListServiceTests : IDisposable
//{
//    private readonly DbContextOptions<AppDbContext> _dbOptions;
//    private readonly Mock<IAuthorizationService> _mockAuthService;
//    private readonly TaskListService _taskListService;

//    public TaskListServiceTests()
//    {
//        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _mockAuthService = new Mock<IAuthorizationService>();
//        _taskListService = new TaskListService(new AppDbContext(_dbOptions), _mockAuthService.Object);
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

//    private TaskList CreateTestTaskList(int id, int boardId, string title = "Test List", int order = 1)
//    {
//        return new TaskList
//        {
//            Id = id,
//            Title = title,  
//            BoardId = boardId,
//            Order = order,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    private Domain.Entities.Task CreateTestTask(int id, int listId, string title = "Test Task")
//    {
//        return new Models.Task
//        {
//            Id = id,
//            Title = title,
//            Description = $"Description for {title}", // Обязательное поле
//            TaskListId = listId,
//            IsCompleted = false, // Обязательное поле
//            Order = 1,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateTaskListAsync_WhenAuthorized_CreatesTaskList()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskListDto { Title = "New Task List" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            context.Boards.Add(CreateTestBoard(boardId, 1));
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskListService.CreateTaskListAsync(createDto, boardId, userId);

//        // Assert
//        Assert.Equal("New Task List", result.Title);
//        Assert.Equal(boardId, result.BoardId);
//        Assert.Equal(1, result.Order); // Первый элемент должен иметь порядок 1

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var taskList = await context.TaskLists.FirstOrDefaultAsync();
//            Assert.NotNull(taskList);
//            Assert.Equal("New Task List", taskList.Title);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateTaskListAsync_WithExistingLists_SetsCorrectOrder()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskListDto { Title = "New Task List" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            board.Lists = new List<TaskList>
//                {
//                    CreateTestTaskList(1, boardId, "List 1", 1),
//                    CreateTestTaskList(2, boardId, "List 2", 2)
//                };
//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskListService.CreateTaskListAsync(createDto, boardId, userId);

//        // Assert
//        Assert.Equal(3, result.Order); // Должен быть следующий порядковый номер

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var taskList = await context.TaskLists
//                .FirstOrDefaultAsync(tl => tl.Title == "New Task List");
//            Assert.NotNull(taskList);
//            Assert.Equal(3, taskList.Order);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task UpdateTaskListAsync_WhenAuthorized_UpdatesTaskList()
//    {
//        // Arrange
//        var taskListId = 1;
//        var boardId = 1;
//        var userId = 1;
//        var updateDto = new UpdateTaskListDto { Title = "Updated List" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(taskListId, boardId);
//            board.Lists = new List<TaskList> { taskList };

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskListService.UpdateTaskListAsync(taskListId, updateDto, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var taskList = await context.TaskLists.FindAsync(taskListId);
//            Assert.Equal("Updated List", taskList.Title);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task DeleteTaskListAsync_WhenAuthorized_DeletesTaskList()
//    {
//        // Arrange
//        var taskListId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(taskListId, boardId);
//            board.Lists = new List<TaskList> { taskList };

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskListService.DeleteTaskListAsync(taskListId, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var taskList = await context.TaskLists.FindAsync(taskListId);
//            Assert.Null(taskList);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetTaskListTasksAsync_WhenAuthorized_ReturnsTasks()
//    {
//        // Arrange
//        var taskListId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(taskListId, boardId);
//            var task = CreateTestTask(1, taskListId); // Теперь используем исправленный метод

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskListService.GetTaskListTasksAsync(taskListId, userId);

//        // Assert
//        var taskDto = Assert.Single(result);
//        Assert.Equal("Test Task", taskDto.Title);
//        Assert.Equal(taskListId, taskDto.TaskListId);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetTaskListTasksAsync_OrdersTasksByOrder()
//    {
//        // Arrange
//        var taskListId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var taskList = CreateTestTaskList(taskListId, boardId);

//            var tasks = new List<Domain.Entities.Task>
//        {
//            new Models.Task
//            {
//                Id = 1,
//                Title = "Task 1",
//                Description = "Description 1", // Добавляем обязательное поле
//                TaskListId = taskListId,
//                Order = 2,
//                CreatedAt = DateTime.UtcNow,
//                IsCompleted = false // Добавляем обязательное поле
//            },
//            new Models.Task
//            {
//                Id = 2,
//                Title = "Task 2",
//                Description = "Description 2", // Добавляем обязательное поле
//                TaskListId = taskListId,
//                Order = 1,
//                CreatedAt = DateTime.UtcNow,
//                IsCompleted = false // Добавляем обязательное поле
//            }
//        };

//            board.Lists = new List<TaskList> { taskList };
//            context.Boards.Add(board);
//            context.Tasks.AddRange(tasks);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _taskListService.GetTaskListTasksAsync(taskListId, userId);

//        // Assert
//        Assert.Equal(2, result.Count());
//        Assert.Equal("Task 2", result.First().Title); // Должен быть первый по порядку
//        Assert.Equal("Task 1", result.Last().Title);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task ReorderTaskListsAsync_WhenAuthorized_UpdatesOrders()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var reorderDto = new ReorderTaskListsDto
//        {
//            ListsOrder = new List<ListOrderUpdateDto>
//        {
//            new ListOrderUpdateDto { ListId = 1, NewPosition = 2 },
//            new ListOrderUpdateDto { ListId = 2, NewPosition = 1 }
//        }
//        };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            board.Lists = new List<TaskList>
//        {
//            CreateTestTaskList(1, boardId, "List 1", 1),
//            CreateTestTaskList(2, boardId, "List 2", 2)
//        };

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _taskListService.ReorderTaskListsAsync(boardId, reorderDto, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var lists = await context.TaskLists
//                .Where(tl => tl.BoardId == boardId)
//                .OrderBy(tl => tl.Order)
//                .ToListAsync();

//            Assert.Equal(2, lists.Count);
//            Assert.Equal("List 2", lists[0].Title); // Теперь должен быть первым
//            Assert.Equal("List 1", lists[1].Title);
//            Assert.Equal(1, lists[0].Order);
//            Assert.Equal(2, lists[1].Order);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateTaskListAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskListDto { Title = "New Task List" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
//            _taskListService.CreateTaskListAsync(createDto, boardId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task UpdateTaskListAsync_WhenTaskListNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var taskListId = 999;
//        var userId = 1;
//        var updateDto = new UpdateTaskListDto { Title = "Updated List" };

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
//            _taskListService.UpdateTaskListAsync(taskListId, updateDto, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetTaskListTasksAsync_WhenTaskListNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var taskListId = 999;
//        var userId = 1;

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
//            _taskListService.GetTaskListTasksAsync(taskListId, userId));
//    }
//}