using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByBoardAndUserIdAsync(int boardId, int userId);
    Task<bool> ExistsAsync(int boardId, int userId);
    System.Threading.Tasks.Task AddAsync(Member member);
    System.Threading.Tasks.Task DeleteAsync(Member member);
}