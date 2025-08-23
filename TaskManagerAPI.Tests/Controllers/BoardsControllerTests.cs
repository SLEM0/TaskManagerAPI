//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Controllers;
//using TaskManagerAPI.Domain.Enums;
//using TaskManagerAPI.Dtos.Board;
//using TaskManagerAPI.Dtos.TaskList;

//namespace TaskManagerAPI.Tests.Controllers;

//public class BoardsControllerTests
//{
//    private readonly Mock<IBoardsService> _mockBoardsService;
//    private readonly Mock<IUserContext> _mockUserContext;
//    private readonly BoardsController _controller;

//    public BoardsControllerTests()
//    {
//        _mockBoardsService = new Mock<IBoardsService>();
//        _mockUserContext = new Mock<IUserContext>();
//        _controller = new BoardsController(_mockBoardsService.Object, _mockUserContext.Object);
//    }

//    [Fact]
//    public async Task GetAllBoards_ReturnsOkResultWithBoards()
//    {
//        // Arrange
//        var userId = 1;
//        var expectedBoards = new List<BoardResponseDto>
//            {
//                new BoardResponseDto { Id = 1, Title = "Test Board 1" },
//                new BoardResponseDto { Id = 2, Title = "Test Board 2" }
//            };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.GetUserBoardsAsync(userId))
//            .ReturnsAsync(expectedBoards);

//        // Act
//        var result = await _controller.GetAllBoards();

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var boards = Assert.IsAssignableFrom<IEnumerable<BoardResponseDto>>(okResult.Value);
//        Assert.Equal(2, boards.Count());
//    }

//    [Fact]
//    public async Task GetBoard_WhenExists_ReturnsOkResult()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var expectedBoard = new BoardResponseDto { Id = boardId, Title = "Test Board" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.GetBoardDetailsAsync(boardId, userId))
//            .ReturnsAsync(expectedBoard);

//        // Act
//        var result = await _controller.GetBoard(boardId);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var board = Assert.IsType<BoardResponseDto>(okResult.Value);
//        Assert.Equal(boardId, board.Id);
//    }

//    [Fact]
//    public async Task GetBoard_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.GetBoardDetailsAsync(boardId, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.GetBoard(boardId);

//        // Assert
//        Assert.IsType<ForbidResult>(result);
//    }

//    [Fact]
//    public async Task GetBoard_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.GetBoardDetailsAsync(boardId, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.GetBoard(boardId);

//        // Assert
//        Assert.IsType<NotFoundResult>(result);
//    }

//    [Fact]
//    public async Task CreateBoard_ReturnsCreatedAtActionResult()
//    {
//        // Arrange
//        var userId = 1;
//        var createDto = new BoardRequestDto { Title = "New Board" };
//        var expectedBoard = new BoardResponseDto { Id = 1, Title = "New Board" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.CreateBoardAsync(createDto, userId))
//            .ReturnsAsync(expectedBoard);

//        // Act
//        var result = await _controller.CreateBoard(createDto);

//        // Assert
//        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
//        Assert.Equal(nameof(BoardsController.GetBoard), createdAtResult.ActionName);
//        Assert.Equal(expectedBoard.Id, ((BoardResponseDto)createdAtResult.Value).Id);
//    }

//    [Fact]
//    public async Task UpdateBoard_WhenSuccess_ReturnsOkWithBoardResponse()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var updateDto = new BoardRequestDto { Title = "Updated Title", Description = "Updated Desc" };
//        var expectedResponse = new BoardResponseDto
//        {
//            Id = boardId,
//            Title = "Updated Title",
//            Description = "Updated Desc",
//            IsOwner = true,
//            OwnerId = userId
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.UpdateBoardAsync(boardId, updateDto, userId))
//            .ReturnsAsync(expectedResponse);

//        // Act
//        var result = await _controller.UpdateBoard(boardId, updateDto);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var returnedBoard = Assert.IsType<BoardResponseDto>(okResult.Value);
//        Assert.Equal("Updated Title", returnedBoard.Title);
//    }

//    [Fact]
//    public async Task UpdateBoard_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var updateDto = new BoardRequestDto { Title = "Updated Title" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.UpdateBoardAsync(boardId, updateDto, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.UpdateBoard(boardId, updateDto);

//        // Assert
//        Assert.IsType<ForbidResult>(result);
//    }

//    [Fact]
//    public async Task DeleteBoard_WhenSuccess_ReturnsNoContent()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.DeleteBoardAsync(boardId, userId))
//            .Returns(System.Threading.Tasks.Task.CompletedTask);

//        // Act
//        var result = await _controller.DeleteBoard(boardId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task GetBoardLists_ReturnsOkResultWithLists()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var expectedLists = new List<TaskListDetailsDto>
//            {
//                new TaskListDetailsDto { Id = 1, Title = "To Do" },
//                new TaskListDetailsDto { Id = 2, Title = "In Progress" }
//            };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.GetBoardListsAsync(boardId, userId))
//            .ReturnsAsync(expectedLists);

//        // Act
//        var result = await _controller.GetBoardLists(boardId);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var lists = Assert.IsAssignableFrom<IEnumerable<TaskListDetailsDto>>(okResult.Value);
//        Assert.Equal(2, lists.Count());
//    }

//    [Fact]
//    public async Task GetBoardMembers_ReturnsOkResultWithMembers()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var expectedMembers = new List<MemberResponseDto>
//            {
//                new MemberResponseDto { UserId = 1, UserName = "User1" },
//                new MemberResponseDto { UserId = 2, UserName = "User2" }
//            };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.GetBoardMembersAsync(boardId, userId))
//            .ReturnsAsync(expectedMembers);

//        // Act
//        var result = await _controller.GetBoardMembers(boardId);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var members = Assert.IsAssignableFrom<IEnumerable<MemberResponseDto>>(okResult.Value);
//        Assert.Equal(2, members.Count());
//    }

//    [Fact]
//    public async Task AddBoardMember_WhenSuccess_ReturnsCreatedWithMemberResponse()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var addMemberDto = new MemberRequestDto { UserId = 2, Role = BoardRole.Editor };
//        var expectedResponse = new MemberResponseDto
//        {
//            Id = 1,
//            BoardId = boardId,
//            UserId = 2,
//            UserName = "testuser",
//            Role = BoardRole.Editor
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.AddBoardMemberAsync(boardId, addMemberDto, userId))
//            .ReturnsAsync(expectedResponse);

//        // Act
//        var result = await _controller.AddBoardMember(boardId, addMemberDto);

//        // Assert
//        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
//        var returnedMember = Assert.IsType<MemberResponseDto>(createdResult.Value);
//        Assert.Equal(2, returnedMember.UserId);
//        Assert.Equal(BoardRole.Editor, returnedMember.Role);
//    }

//    [Fact]
//    public async Task AddBoardMember_WhenUserNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var addMemberDto = new MemberRequestDto { UserId = 999, Role = BoardRole.Editor };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.AddBoardMemberAsync(boardId, addMemberDto, userId))
//            .ThrowsAsync(new KeyNotFoundException("User not found"));

//        // Act
//        var result = await _controller.AddBoardMember(boardId, addMemberDto);

//        // Assert
//        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//        Assert.Equal("User not found", notFoundResult.Value);
//    }

//    [Fact]
//    public async Task RemoveBoardMember_WhenSuccess_ReturnsNoContent()
//    {
//        // Arrange
//        var boardId = 1;
//        var memberId = 2;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.RemoveBoardMemberAsync(boardId, memberId, userId))
//            .Returns(System.Threading.Tasks.Task.CompletedTask);

//        // Act
//        var result = await _controller.RemoveBoardMember(boardId, memberId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task RemoveBoardMember_WhenTryingToRemoveSelf_ReturnsBadRequest()
//    {
//        // Arrange
//        var boardId = 1;
//        var memberId = 1; // same as current user
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockBoardsService.Setup(x => x.RemoveBoardMemberAsync(boardId, memberId, userId))
//            .ThrowsAsync(new InvalidOperationException("Cannot remove yourself"));

//        // Act
//        var result = await _controller.RemoveBoardMember(boardId, memberId);

//        // Assert
//        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//        Assert.Equal("Cannot remove yourself", badRequestResult.Value);
//    }
//}