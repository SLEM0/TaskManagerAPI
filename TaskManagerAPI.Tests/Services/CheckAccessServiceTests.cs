using Moq;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class CheckAccessServiceTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CheckAccessService _checkAccessService;

    public CheckAccessServiceTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _userContextMock = new Mock<IUserContext>();

        _checkAccessService = new CheckAccessService(
            _boardRepositoryMock.Object,
            _userContextMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_BoardNotFound_ReturnsNoAccess()
    {
        // Arrange
        var boardId = 1;
        var userId = 1;

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync((Board)null);

        // Act
        var result = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);

        // Assert
        Assert.False(result.hasAccess);
        Assert.False(result.isOwner);
        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Once);
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_UserIsOwner_ReturnsFullAccess()
    {
        // Arrange
        var boardId = 1;
        var userId = 1;
        var board = new Board
        {
            Id = boardId,
            OwnerId = userId,
            BoardUsers = new List<Member>()
        };

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var resultViewer = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        var resultEditor = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        var resultOwner = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

        // Assert
        Assert.True(resultViewer.hasAccess);
        Assert.True(resultViewer.isOwner);

        Assert.True(resultEditor.hasAccess);
        Assert.True(resultEditor.isOwner);

        Assert.True(resultOwner.hasAccess);
        Assert.True(resultOwner.isOwner);

        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Exactly(3));
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Exactly(3));
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_UserIsEditor_HasEditorAndViewerAccess()
    {
        // Arrange
        var boardId = 1;
        var userId = 2;
        var ownerId = 1;
        var board = new Board
        {
            Id = boardId,
            OwnerId = ownerId,
            BoardUsers = new List<Member>
            {
                new Member { UserId = userId, Role = BoardRole.Editor }
            }
        };

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var resultViewer = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        var resultEditor = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        var resultOwner = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

        // Assert
        Assert.True(resultViewer.hasAccess);
        Assert.False(resultViewer.isOwner);

        Assert.True(resultEditor.hasAccess);
        Assert.False(resultEditor.isOwner);

        Assert.False(resultOwner.hasAccess);
        Assert.False(resultOwner.isOwner);

        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Exactly(3));
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Exactly(3));
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_UserIsViewer_HasOnlyViewerAccess()
    {
        // Arrange
        var boardId = 1;
        var userId = 2;
        var ownerId = 1;
        var board = new Board
        {
            Id = boardId,
            OwnerId = ownerId,
            BoardUsers = new List<Member>
            {
                new Member { UserId = userId, Role = BoardRole.Viewer }
            }
        };

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var resultViewer = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        var resultEditor = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        var resultOwner = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

        // Assert
        Assert.True(resultViewer.hasAccess);
        Assert.False(resultViewer.isOwner);

        Assert.False(resultEditor.hasAccess);
        Assert.False(resultEditor.isOwner);

        Assert.False(resultOwner.hasAccess);
        Assert.False(resultOwner.isOwner);

        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Exactly(3));
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Exactly(3));
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_UserNotMember_ReturnsNoAccess()
    {
        // Arrange
        var boardId = 1;
        var userId = 3;
        var ownerId = 1;
        var board = new Board
        {
            Id = boardId,
            OwnerId = ownerId,
            BoardUsers = new List<Member>
            {
                new Member { UserId = 2, Role = BoardRole.Editor }
            }
        };

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var resultViewer = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        var resultEditor = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        var resultOwner = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Owner);

        // Assert
        Assert.False(resultViewer.hasAccess);
        Assert.False(resultViewer.isOwner);

        Assert.False(resultEditor.hasAccess);
        Assert.False(resultEditor.isOwner);

        Assert.False(resultOwner.hasAccess);
        Assert.False(resultOwner.isOwner);

        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Exactly(3));
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Exactly(3));
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_BoardHasNoMembers_OnlyOwnerHasAccess()
    {
        // Arrange
        var boardId = 1;
        var userId = 2;
        var ownerId = 1;
        var board = new Board
        {
            Id = boardId,
            OwnerId = ownerId,
            BoardUsers = new List<Member>()
        };

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(userId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var result = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);

        // Assert
        Assert.False(result.hasAccess);
        Assert.False(result.isOwner);
        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Once);
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckBoardAccessAsync_MultipleMembers_CorrectAccessForEach()
    {
        // Arrange
        var boardId = 1;
        var ownerId = 1;
        var editorId = 2;
        var viewerId = 3;

        var board = new Board
        {
            Id = boardId,
            OwnerId = ownerId,
            BoardUsers = new List<Member>
            {
                new Member { UserId = editorId, Role = BoardRole.Editor },
                new Member { UserId = viewerId, Role = BoardRole.Viewer }
            }
        };

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(editorId);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var editorResult = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Editor);

        _userContextMock.Setup(x => x.GetCurrentUserId())
            .Returns(viewerId);

        var viewerResult = await _checkAccessService.CheckBoardAccessAsync(boardId, BoardRole.Editor);

        // Assert
        Assert.True(editorResult.hasAccess);
        Assert.False(editorResult.isOwner);

        Assert.False(viewerResult.hasAccess);
        Assert.False(viewerResult.isOwner);

        _userContextMock.Verify(x => x.GetCurrentUserId(), Times.Exactly(2));
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Exactly(2));
    }
}