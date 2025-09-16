using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class TaskListRepository : ITaskListRepository
{
    private readonly AppDbContext _context;

    public TaskListRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskList?> GetByIdAsync(int id)
    {
        return await _context.TaskLists.FindAsync(id);
    }

    public async Task<TaskList?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.TaskLists
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Labels)
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.User)
            .Include(tl => tl.Board)
            .AsSplitQuery()
            .FirstOrDefaultAsync(tl => tl.Id == id);
    }

    public async Task<TaskList?> GetByIdWithBoardAsync(int id)
    {
        return await _context.TaskLists
            .Include(tl => tl.Board)
            .FirstOrDefaultAsync(tl => tl.Id == id);
    }

    public async Task<IEnumerable<TaskList>> GetByBoardIdAsync(int boardId)
    {
        return await _context.TaskLists
            .Where(tl => tl.BoardId == boardId)
            .OrderBy(tl => tl.Order)
            .ToListAsync();
    }

    public async Task<int> GetMaxOrderAsync(int boardId)
    {
        return await _context.TaskLists
            .Where(tl => tl.BoardId == boardId)
            .OrderByDescending(tl => tl.Order)
            .Select(tl => tl.Order)
            .FirstOrDefaultAsync();
    }

    public async System.Threading.Tasks.Task AddAsync(TaskList taskList)
    {
        await _context.TaskLists.AddAsync(taskList);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateAsync(TaskList taskList)
    {
        _context.TaskLists.Update(taskList);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(TaskList taskList)
    {
        _context.TaskLists.Remove(taskList);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateRangeAsync(IEnumerable<TaskList> taskLists)
    {
        _context.TaskLists.UpdateRange(taskLists);
        await _context.SaveChangesAsync();
    }
}