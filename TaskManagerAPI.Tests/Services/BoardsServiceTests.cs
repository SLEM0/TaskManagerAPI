//using Microsoft.EntityFrameworkCore;
//using Moq;
//using TaskManagerAPI.Application.Dtos.Board;
//using TaskManagerAPI.Application.Dtos.Member;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Domain.Entities;
//using TaskManagerAPI.Domain.Enums;
//using TaskManagerAPI.Infrastructure.Data;
//using TaskManagerAPI.Infrastructure.Services;

//namespace TaskManagerAPI.Tests.Services;

//public class BoardServiceTests
//{
//    private readonly Mock<AppDbContext> _mockContext;
//    private readonly Mock<IAuthorizationService> _mockAuthService;
//    private readonly BoardService _boardService;

//    public BoardServiceTests()
//    {
//        _mockContext = new Mock<AppDbContext>();
//        _mockAuthService = new Mock<IAuthorizationService>();
//        _boardService = new BoardService(_mockContext.Object, _mockAuthService.Object);
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateBoardAsync_UserNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var dto = new BoardRequestDto { Title = "Test", Description = "Test" };
//        var userId = 1;

//        var mockUsers = new Mock<DbSet<User>>();
//        mockUsers.Setup(x => x.FindAsync(userId)).ReturnsAsync((User)null);
//        _mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() => _boardService.CreateBoardAsync(dto, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task CreateBoardAsync_ValidData_CreatesBoard()
//    {
//        // Arrange
//        var dto = new BoardRequestDto { Title = "Test", Description = "Test" };
//        var userId = 1;
//        var user = new User { Id = userId, Username = "test", Email = "test@test.com" };

//        var mockUsers = new Mock<DbSet<User>>();
//        var mockBoards = new Mock<DbSet<Board>>();
//        var mockBoardUsers = new Mock<DbSet<BoardUser>>();

//        mockUsers.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);
//        _mockContext.Setup(x => x.Users).Returns(mockUsers.Object);
//        _mockContext.Setup(x => x.Boards).Returns(mockBoards.Object);
//        _mockContext.Setup(x => x.BoardUsers).Returns(mockBoardUsers.Object);
//        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

//        // Act
//        var result = await _boardService.CreateBoardAsync(dto, userId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(dto.Title, result.Title);
//        mockBoards.Verify(x => x.Add(It.IsAny<Board>()), Times.Once);
//        mockBoardUsers.Verify(x => x.Add(It.IsAny<BoardUser>()), Times.Once);
//        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetBoardDetailsAsync_NoAccess_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _boardService.GetBoardDetailsAsync(boardId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task GetBoardDetailsAsync_BoardNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
//            .ReturnsAsync((true, true));

//        var mockBoards = new Mock<DbSet<Board>>();
//        mockBoards.Setup(x => x.FindAsync(boardId)).ReturnsAsync((Board)null);
//        _mockContext.Setup(x => x.Boards).Returns(mockBoards.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() => _boardService.GetBoardDetailsAsync(boardId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task UpdateBoardAsync_NoOwnerAccess_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var dto = new BoardRequestDto { Title = "Updated", Description = "Updated" };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _boardService.UpdateBoardAsync(boardId, dto, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task DeleteBoardAsync_NoOwnerAccess_ThrowsUnauthorizedAccessException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
//            .ReturnsAsync((false, false));

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _boardService.DeleteBoardAsync(boardId, userId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task AddBoardMemberAsync_UserNotFound_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var boardId = 1;
//        var requestingUserId = 1;
//        var dto = new MemberRequestDto { UserId = 2, Role = BoardRole.Editor };

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
//            .ReturnsAsync((true, true));

//        var mockUsers = new Mock<DbSet<User>>();
//        mockUsers.Setup(x => x.FindAsync(dto.UserId)).ReturnsAsync((User)null);
//        _mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<KeyNotFoundException>(() => _boardService.AddBoardMemberAsync(boardId, dto, requestingUserId));
//    }

//    [Fact]
//    public async System.Threading.Tasks.Task RemoveBoardMemberAsync_TryRemoveSelf_ThrowsInvalidOperationException()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var requestingUserId = 1;

//        _mockAuthService.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
//            .ReturnsAsync((true, true));

//        var boardUser = new BoardUser { UserId = userId };
//        var mockBoardUsers = new Mock<DbSet<BoardUser>>();

//        mockBoardUsers.Setup(x => x.FirstOrDefaultAsync(
//            It.IsAny<System.Linq.Expressions.Expression<Func<BoardUser, bool>>>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(boardUser);

//        _mockContext.Setup(x => x.BoardUsers).Returns(mockBoardUsers.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<InvalidOperationException>(() => _boardService.RemoveBoardMemberAsync(boardId, userId, requestingUserId));
//    }
//}