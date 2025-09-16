using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface ICheckAccessService
{
    Task<(bool hasAccess, bool isOwner)> CheckBoardAccessAsync(int boardId, BoardRole requiredRole);
}