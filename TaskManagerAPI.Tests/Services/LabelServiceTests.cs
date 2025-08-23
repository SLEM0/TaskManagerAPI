//using Microsoft.EntityFrameworkCore;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Domain.Entities;
//using TaskManagerAPI.Domain.Enums;
//using TaskManagerAPI.Dtos.Label;
//using TaskManagerAPI.Infrastructure.Data;
//using TaskManagerAPI.Infrastructure.Services;

//namespace TaskManagerAPI.Tests.Services;

//public class LabelServiceTests : IDisposable
//{
//    private readonly DbContextOptions<AppDbContext> _dbOptions;
//    private readonly Mock<IAuthorizationService> _mockAuthService;
//    private readonly LabelService _labelService;

//    public LabelServiceTests()
//    {
//        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _mockAuthService = new Mock<IAuthorizationService>();
//        _labelService = new LabelService(new AppDbContext(_dbOptions), _mockAuthService.Object);
//    }

//    public void Dispose()
//    {
//        using var context = new AppDbContext(_dbOptions);
//        context.Database.EnsureDeleted();
//    }

//    private Board CreateTestBoard(int id, int ownerId, List<Label> labels = null)
//    {
//        return new Board
//        {
//            Id = id,
//            Title = $"Test Board {id}",
//            Description = $"Description {id}",
//            OwnerId = ownerId,
//            CreatedAt = DateTime.UtcNow,
//            Labels = labels ?? new List<Label>()
//        };
//    }

//    private Label CreateTestLabel(int id, int boardId, string name = "Test Label", string color = "#FF0000")
//    {
//        return new Label
//        {
//            Id = id,
//            Name = name,
//            Color = color,
//            BoardId = boardId,
//            CreatedAt = DateTime.UtcNow
//        };
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetBoardLabelsAsync_WhenAuthorized_ReturnsLabels()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1, new List<Label>
//                {
//                    CreateTestLabel(1, boardId, "Label 1"),
//                    CreateTestLabel(2, boardId, "Label 2")
//                });
//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _labelService.GetBoardLabelsAsync(boardId, userId);

//        // Assert
//        Assert.Equal(2, result.Count());
//        Assert.Contains(result, l => l.Name == "Label 1");
//        Assert.Contains(result, l => l.Name == "Label 2");
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetBoardLabelsAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
//            _labelService.GetBoardLabelsAsync(boardId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetLabelDetailsAsync_WhenAuthorized_ReturnsLabelDetails()
//    {
//        // Arrange
//        var labelId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var label = CreateTestLabel(labelId, boardId);
//            context.Labels.Add(label);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _labelService.GetLabelDetailsAsync(labelId, userId);

//        // Assert
//        Assert.Equal(labelId, result.Id);
//        Assert.Equal("Test Label", result.Name);
//        Assert.Equal("#FF0000", result.Color);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetLabelDetailsAsync_WhenLabelNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var labelId = 999;
//        var userId = 1;

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
//            _labelService.GetLabelDetailsAsync(labelId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateLabelAsync_WhenAuthorized_CreatesLabel()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateLabelDto { Name = "New Label", Color = "#00FF00" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            context.Boards.Add(CreateTestBoard(boardId, 1));
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _labelService.CreateLabelAsync(createDto, boardId, userId);

//        // Assert
//        Assert.Equal("New Label", result.Name);
//        Assert.Equal("#00FF00", result.Color);
//        Assert.Equal(0, result.TaskCount);

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var label = await context.Labels.FirstOrDefaultAsync();
//            Assert.NotNull(label);
//            Assert.Equal(boardId, label.BoardId);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateLabelAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateLabelDto { Name = "New Label", Color = "#00FF00" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
//            _labelService.CreateLabelAsync(createDto, boardId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task UpdateLabelAsync_WhenAuthorized_UpdatesLabel()
//    {
//        // Arrange
//        var labelId = 1;
//        var boardId = 1;
//        var userId = 1;
//        var updateDto = new UpdateLabelDto { Name = "Updated Label", Color = "#0000FF" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            // Создаем доску и добавляем в нее метку
//            var board = CreateTestBoard(boardId, 1);
//            var label = CreateTestLabel(labelId, boardId);
//            board.Labels = new List<Label> { label }; // Устанавливаем связь

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _labelService.UpdateLabelAsync(labelId, updateDto, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var label = await context.Labels.FindAsync(labelId);
//            Assert.Equal("Updated Label", label.Name);
//            Assert.Equal("#0000FF", label.Color);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task UpdateLabelAsync_WhenLabelNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var labelId = 999;
//        var userId = 1;
//        var updateDto = new UpdateLabelDto { Name = "Updated Label", Color = "#0000FF" };

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
//            _labelService.UpdateLabelAsync(labelId, updateDto, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task DeleteLabelAsync_WhenAuthorized_DeletesLabel()
//    {
//        // Arrange
//        var labelId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            // Создаем доску и добавляем в нее метку
//            var board = CreateTestBoard(boardId, 1);
//            var label = CreateTestLabel(labelId, boardId);
//            board.Labels = new List<Label> { label }; // Устанавливаем связь

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        await _labelService.DeleteLabelAsync(labelId, userId);

//        // Assert
//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var label = await context.Labels.FindAsync(labelId);
//            Assert.Null(label);
//        }
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetLabelTasksAsync_WhenAuthorized_ReturnsLabelTasks()
//    {
//        // Arrange
//        var labelId = 1;
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, false));

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            // Создаем доску и добавляем в нее метку
//            var board = CreateTestBoard(boardId, 1);
//            var label = CreateTestLabel(labelId, boardId);
//            board.Labels = new List<Label> { label }; // Устанавливаем связь

//            var task = new Models.Task
//            {
//                Id = 1,
//                Title = "Test Task",
//                Description = "Task Description",
//                IsCompleted = false,
//                CreatedAt = DateTime.UtcNow,
//                TaskListId = 1,
//                Labels = new List<Label> { label }
//            };

//            context.Boards.Add(board);
//            context.Tasks.Add(task);
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var result = await _labelService.GetLabelTasksAsync(labelId, userId);

//        // Assert
//        var taskDto = Assert.Single(result);
//        Assert.Equal("Test Task", taskDto.Title);
//        Assert.Equal("Task Description", taskDto.Description);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetLabelTasksAsync_WhenLabelNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var labelId = 999;
//        var userId = 1;

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
//            _labelService.GetLabelTasksAsync(labelId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetLabelTasksAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var labelId = 1;
//        var boardId = 1;
//        var userId = 1;

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            var board = CreateTestBoard(boardId, 1);
//            var label = CreateTestLabel(labelId, boardId);
//            board.Labels = new List<Label> { label };

//            context.Boards.Add(board);
//            await context.SaveChangesAsync();
//        }

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
//            _labelService.GetLabelTasksAsync(labelId, userId));
//    }
//}