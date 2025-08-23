using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Application.Interfaces;

public interface ICheckAccessService
{
    Task<(bool hasAccess, bool isOwner)> CheckBoardAccessAsync(int boardId, BoardRole requiredRole);
    Task<(bool hasAccess, bool isOwner)> CheckTaskListAccessAsync(int taskListId, BoardRole requiredRole);
}