using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;

    public BoardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Board?> GetByIdAsync(int id)
    {
        return await _context.Boards.FindAsync(id);
    }

    public async Task<Board?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Boards
            .Include(b => b.Owner)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Labels)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
            .Include(b => b.Labels)
            .Include(b => b.BoardUsers)
                .ThenInclude(bu => bu.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Board>> GetUserBoardsAsync(int userId)
    {
        return await _context.Boards
            .Where(b => b.OwnerId == userId || b.BoardUsers.Any(bu => bu.UserId == userId))
            .Include(b => b.Owner)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task AddAsync(Board board)
    {
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateAsync(Board board)
    {
        _context.Boards.Update(board);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(Board board)
    {
        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Boards.AnyAsync(b => b.Id == id);
    }

    public async Task<Board?> GetBoardWithTasksAsync(int id)
    {
        return await _context.Boards
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Labels)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}