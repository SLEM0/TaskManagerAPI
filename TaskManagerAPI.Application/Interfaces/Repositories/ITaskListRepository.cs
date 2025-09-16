using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface ITaskListRepository
{
    Task<TaskList?> GetByIdAsync(int id);
    Task<TaskList?> GetByIdWithDetailsAsync(int id);
    Task<TaskList?> GetByIdWithBoardAsync(int id);
    Task<IEnumerable<TaskList>> GetByBoardIdAsync(int boardId);
    Task<int> GetMaxOrderAsync(int boardId);
    System.Threading.Tasks.Task AddAsync(TaskList taskList);
    System.Threading.Tasks.Task UpdateAsync(TaskList taskList);
    System.Threading.Tasks.Task DeleteAsync(TaskList taskList);
    System.Threading.Tasks.Task UpdateRangeAsync(IEnumerable<TaskList> taskLists);
}