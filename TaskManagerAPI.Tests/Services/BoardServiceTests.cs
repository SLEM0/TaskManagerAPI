using AutoMapper;
using Moq;
using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class BoardServiceTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<ICheckAccessService> _authServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly IMapper _mapper;
    private readonly BoardService _boardService;

    public BoardServiceTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _authServiceMock = new Mock<ICheckAccessService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _memberRepositoryMock = new Mock<IMemberRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Board, BoardResponseDto>();
            cfg.CreateMap<Board, ShortBoardResponseDto>();
        });
        _mapper = config.CreateMapper();

        _boardService = new BoardService(
            _boardRepositoryMock.Object,
            _authServiceMock.Object,
            _userRepositoryMock.Object,
            _memberRepositoryMock.Object,
            _mapper);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateBoardAsync_ValidRequest_ReturnsBoardResponse()
    {
        // Arrange
        var userId = 1;
        var dto = new BoardRequestDto { Title = "Test Board", Description = "Test Description" };
        var user = new User { Id = userId, Email = "test@test.com" };
        var board = new Board { Id = 1, Title = dto.Title, Description = dto.Description, OwnerId = userId };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _boardRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Board>()))
            .Callback<Board>(b => b.Id = 1);
        _memberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Member>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _boardRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(board);

        // Act
        var result = await _boardService.CreateBoardAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(board.Id, result.Id);
        Assert.Equal(board.Title, result.Title);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _boardRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Board>()), Times.Once);
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateBoardAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 1;
        var dto = new BoardRequestDto { Title = "Test Board" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _boardService.CreateBoardAsync(dto, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetBoardDetailsAsync_ValidAccess_ReturnsBoardDetails()
    {
        // Arrange
        var boardId = 1;
        var board = new Board { Id = boardId, Title = "Test Board", OwnerId = 1 };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((true, true));
        _boardRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var result = await _boardService.GetBoardDetailsAsync(boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(board.Id, result.Id);
        Assert.Equal(board.Title, result.Title);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer), Times.Once);
        _boardRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(boardId), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetBoardDetailsAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var boardId = 1;

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _boardService.GetBoardDetailsAsync(boardId));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateBoardAsync_ValidRequest_UpdatesBoard()
    {
        // Arrange
        var boardId = 1;
        var dto = new BoardRequestDto { Title = "Updated Board", Description = "Updated Description" };
        var board = new Board { Id = boardId, Title = "Old Title", Description = "Old Description", OwnerId = 1 };
        var updatedBoard = new Board { Id = boardId, Title = dto.Title, Description = dto.Description, OwnerId = 1 };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);
        _boardRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Board>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _boardRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(boardId))
            .ReturnsAsync(updatedBoard);

        // Act
        var result = await _boardService.UpdateBoardAsync(boardId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner), Times.Once);
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Once);
        _boardRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Board>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteBoardAsync_ValidRequest_DeletesBoard()
    {
        // Arrange
        var boardId = 1;
        var board = new Board { Id = boardId };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);
        _boardRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Board>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _boardService.DeleteBoardAsync(boardId);

        // Assert
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner), Times.Once);
        _boardRepositoryMock.Verify(x => x.GetByIdAsync(boardId), Times.Once);
        _boardRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Board>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddBoardMemberAsync_ValidRequest_AddsMember()
    {
        // Arrange
        var boardId = 1;
        var userId = 2;
        var ownerId = 1;
        var dto = new MemberRequestDto { Email = "newuser@test.com", Role = BoardRole.Editor };
        var user = new User { Id = userId, Email = dto.Email.ToLower() };
        var board = new Board { Id = boardId, OwnerId = ownerId };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync(user);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);
        _memberRepositoryMock.Setup(x => x.ExistsAsync(boardId, userId))
            .ReturnsAsync(false);
        _memberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Member>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _boardRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(boardId))
            .ReturnsAsync(board);

        // Act
        var result = await _boardService.AddBoardMemberAsync(boardId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.Id);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(dto.Email.ToLower()), Times.Once);
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddBoardMemberAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var boardId = 1;
        var dto = new MemberRequestDto { Email = "nonexistent@test.com" };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _boardService.AddBoardMemberAsync(boardId, dto));
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveBoardMemberAsync_ValidRequest_RemovesMember()
    {
        // Arrange
        var boardId = 1;
        var userId = 2;
        var requestingUserId = 1;
        var member = new Member { BoardId = boardId, UserId = userId };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _memberRepositoryMock.Setup(x => x.GetByBoardAndUserIdAsync(boardId, userId))
            .ReturnsAsync(member);
        _memberRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Member>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _boardService.RemoveBoardMemberAsync(boardId, userId, requestingUserId);

        // Assert
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner), Times.Once);
        _memberRepositoryMock.Verify(x => x.GetByBoardAndUserIdAsync(boardId, userId), Times.Once);
        _memberRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveBoardMemberAsync_TryRemoveSelf_ThrowsValidationException()
    {
        // Arrange
        var boardId = 1;
        var userId = 1;
        var requestingUserId = 1;

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _boardService.RemoveBoardMemberAsync(boardId, userId, requestingUserId));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddBoardMemberAsync_UserIsOwner_ThrowsValidationException()
    {
        // Arrange
        var boardId = 1;
        var ownerId = 1;
        var dto = new MemberRequestDto { Email = "owner@test.com", Role = BoardRole.Editor };
        var user = new User { Id = ownerId, Email = dto.Email.ToLower() };
        var board = new Board { Id = boardId, OwnerId = ownerId };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync(user);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _boardService.AddBoardMemberAsync(boardId, dto));
    }

    [Fact]
    public async System.Threading.Tasks.Task AddBoardMemberAsync_UserAlreadyMember_ThrowsValidationException()
    {
        // Arrange
        var boardId = 1;
        var userId = 2;
        var ownerId = 1;
        var dto = new MemberRequestDto { Email = "member@test.com", Role = BoardRole.Editor };
        var user = new User { Id = userId, Email = dto.Email.ToLower() };
        var board = new Board { Id = boardId, OwnerId = ownerId };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Owner))
            .ReturnsAsync((true, true));
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email.ToLower()))
            .ReturnsAsync(user);
        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId))
            .ReturnsAsync(board);
        _memberRepositoryMock.Setup(x => x.ExistsAsync(boardId, userId))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _boardService.AddBoardMemberAsync(boardId, dto));
    }
}