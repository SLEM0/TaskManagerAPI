namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface ITaskRepository
{
    Task<Domain.Entities.Task?> GetByIdAsync(int id);
    Task<Domain.Entities.Task?> GetByIdWithDetailsAsync(int id);
    Task<Domain.Entities.Task?> GetByIdWithTaskListAsync(int id);
    Task<int> GetMaxOrderAsync(int taskListId);
    Task<IEnumerable<Domain.Entities.Task>> GetByTaskListIdAsync(int taskListId);
    Task<IEnumerable<Domain.Entities.Task>> GetTasksDueBetweenAsync(DateTime start, DateTime end);
    Task AddAsync(Domain.Entities.Task task);
    Task UpdateAsync(Domain.Entities.Task task);
    Task UpdateRangeAsync(IEnumerable<Domain.Entities.Task> tasks);
    Task DeleteAsync(Domain.Entities.Task task);
}