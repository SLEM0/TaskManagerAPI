using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(int id);
    Task<Board?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Board>> GetUserBoardsAsync(int userId);
    System.Threading.Tasks.Task AddAsync(Board board);
    System.Threading.Tasks.Task UpdateAsync(Board board);
    System.Threading.Tasks.Task DeleteAsync(Board board);
    Task<bool> ExistsAsync(int id);
    Task<Board?> GetBoardWithTasksAsync(int id);
}