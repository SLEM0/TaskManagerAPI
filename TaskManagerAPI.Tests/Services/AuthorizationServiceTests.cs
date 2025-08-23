//using Microsoft.EntityFrameworkCore;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Domain.Entities;
//using TaskManagerAPI.Domain.Enums;
//using TaskManagerAPI.Infrastructure.Data;
//using TaskManagerAPI.Infrastructure.Services;

//namespace TaskManagerAPI.Tests.Services;

//public class AuthorizationServiceTests : IDisposable
//{
//    private readonly DbContextOptions<AppDbContext> _dbOptions;
//    private readonly Mock<IUserContext> _mockUserContext;
//    private readonly AuthorizationService _authService;

//    public AuthorizationServiceTests()
//    {
//        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        _mockUserContext = new Mock<IUserContext>();
//        _authService = new AuthorizationService(new AppDbContext(_dbOptions), _mockUserContext.Object);
//    }

//    public void Dispose()
//    {
//        using var context = new AppDbContext(_dbOptions);
//        context.Database.EnsureDeleted();
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CheckBoardAccessAsync_WhenUserIsOwner_ReturnsFullAccess()
//    {
//        // Arrange
//        var userId = 1;
//        var boardId = 1;
//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            context.Boards.Add(new Board
//            {
//                Id = boardId,
//                Title = "Test Board", // Добавляем обязательное поле
//                Description = "Test Description", // Добавляем обязательное поле
//                OwnerId = userId,
//                CreatedAt = DateTime.UtcNow // Добавляем обязательное поле, если требуется
//            });
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

//        // Assert
//        Assert.True(hasAccess);
//        Assert.True(isOwner);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CheckBoardAccessAsync_WhenBoardNotFound_ReturnsNoAccess()
//    {
//        // Arrange
//        var userId = 1;
//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

//        // Act
//        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(999, BoardRole.Viewer);

//        // Assert
//        Assert.False(hasAccess);
//        Assert.False(isOwner);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CheckBoardAccessAsync_WhenUserIsEditor_ReturnsCorrectAccess()
//    {
//        // Arrange
//        var userId = 2;
//        var boardId = 1;
//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            context.Boards.Add(new Board
//            {
//                Id = boardId,
//                Title = "Test Board",
//                Description = "Test Description",
//                OwnerId = 1,
//                CreatedAt = DateTime.UtcNow,
//                BoardUsers = new List<BoardUser>
//            {
//                new BoardUser { UserId = userId, Role = BoardRole.Editor }
//            }
//            });
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var (editorAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
//        var (viewerAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
//        var (ownerAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

//        // Assert
//        Assert.True(editorAccess);
//        Assert.True(viewerAccess);
//        Assert.False(ownerAccess);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CheckBoardAccessAsync_WhenUserIsViewer_ReturnsCorrectAccess()
//    {
//        // Arrange
//        var userId = 2;
//        var boardId = 1;
//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            context.Boards.Add(new Board
//            {
//                Id = boardId,
//                Title = "Test Board",
//                Description = "Test Description",
//                OwnerId = 1,
//                CreatedAt = DateTime.UtcNow,
//                BoardUsers = new List<BoardUser>
//            {
//                new BoardUser { UserId = userId, Role = BoardRole.Viewer }
//            }
//            });
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var (editorAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
//        var (viewerAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
//        var (ownerAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

//        // Assert
//        Assert.False(editorAccess);
//        Assert.True(viewerAccess);
//        Assert.False(ownerAccess);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CheckTaskListAccessAsync_WhenTaskListExists_DelegatesToBoardCheck()
//    {
//        // Arrange
//        var userId = 1;
//        var boardId = 1;
//        var taskListId = 1;
//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

//        using (var context = new AppDbContext(_dbOptions))
//        {
//            context.Boards.Add(new Board
//            {
//                Id = boardId,
//                Title = "Test Board",
//                Description = "Test Description",
//                OwnerId = userId,
//                CreatedAt = DateTime.UtcNow,
//                Lists = new List<TaskList>
//                {
//                    new TaskList
//                    {
//                        Id = taskListId,
//                        BoardId = boardId,
//                        Title = "Test List", // Добавляем обязательное поле, если требуется
//                        CreatedAt = DateTime.UtcNow // Добавляем обязательное поле, если требуется
//                    }
//                }
//            });
//            await context.SaveChangesAsync();
//        }

//        // Act
//        var (hasAccess, isOwner) = await _authService.CheckTaskListAccessAsync(taskListId, BoardRole.Owner);

//        // Assert
//        Assert.True(hasAccess);
//        Assert.True(isOwner);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CheckTaskListAccessAsync_WhenTaskListNotFound_ReturnsNoAccess()
//    {
//        // Arrange
//        var userId = 1;
//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

//        // Act
//        var (hasAccess, isOwner) = await _authService.CheckTaskListAccessAsync(999, BoardRole.Viewer);

//        // Assert
//        Assert.False(hasAccess);
//        Assert.False(isOwner);
//    }
//}