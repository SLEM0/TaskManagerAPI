using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class CheckAccessService : ICheckAccessService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUserContext _userContext;

    public CheckAccessService(IBoardRepository boardRepository, IUserContext userContext)
    {
        _boardRepository = boardRepository;
        _userContext = userContext;
    }

    public async Task<(bool hasAccess, bool isOwner)> CheckBoardAccessAsync(int boardId, BoardRole requiredRole)
    {
        var userId = _userContext.GetCurrentUserId();

        var board = await _boardRepository.GetByIdAsync(boardId);

        if (board == null)
            return (false, false);

        if (board.OwnerId == userId)
            return (true, true);

        var userRole = board.BoardUsers
            .FirstOrDefault(bu => bu.UserId == userId)?.Role;

        var hasAccess = requiredRole switch
        {
            BoardRole.Owner => false,
            BoardRole.Editor => userRole is BoardRole.Editor or BoardRole.Owner,
            BoardRole.Viewer => userRole is BoardRole.Viewer or BoardRole.Editor or BoardRole.Owner,
            _ => false
        };

        return (hasAccess, false);
    }
}