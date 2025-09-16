using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Task?> GetByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<Domain.Entities.Task?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.TaskList)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Domain.Entities.Task?> GetByIdWithTaskListAsync(int id)
    {
        return await _context.Tasks
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> GetMaxOrderAsync(int taskListId)
    {
        return await _context.Tasks
            .Where(t => t.TaskListId == taskListId)
            .OrderByDescending(t => t.Order)
            .Select(t => t.Order)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetByTaskListIdAsync(int taskListId)
    {
        return await _context.Tasks
            .Where(t => t.TaskListId == taskListId)
            .OrderBy(t => t.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetTasksDueBetweenAsync(DateTime start, DateTime end)
    {
        return await _context.Tasks
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Where(t => t.DueDate != null &&
                       t.DueDate >= start &&
                       t.DueDate <= end &&
                       !t.IsCompleted &&
                       !t.DueDateNotificationSent)
            .ToListAsync();
    }

    public async Task AddAsync(Domain.Entities.Task task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Domain.Entities.Task task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Domain.Entities.Task> tasks)
    {
        _context.Tasks.UpdateRange(tasks);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Domain.Entities.Task task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}