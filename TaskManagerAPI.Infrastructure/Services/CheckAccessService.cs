using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class CheckAccessService : ICheckAccessService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;

    public CheckAccessService(AppDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<(bool hasAccess, bool isOwner)> CheckBoardAccessAsync(int boardId, BoardRole requiredRole)
    {
        var userId = _userContext.GetCurrentUserId();

        var board = await _context.Boards
            .Include(b => b.BoardUsers)
            .FirstOrDefaultAsync(b => b.Id == boardId);

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

    public async Task<(bool hasAccess, bool isOwner)> CheckTaskListAccessAsync(int taskListId, BoardRole requiredRole)
    {
        var taskList = await _context.TaskLists
            .Include(tl => tl.Board)
            .ThenInclude(b => b.BoardUsers)
            .FirstOrDefaultAsync(tl => tl.Id == taskListId);

        if (taskList == null)
            return (false, false);

        return await CheckBoardAccessAsync(taskList.BoardId, requiredRole);
    }
}