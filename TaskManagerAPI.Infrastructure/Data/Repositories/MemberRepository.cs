using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly AppDbContext _context;

    public MemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByBoardAndUserIdAsync(int boardId, int userId)
    {
        return await _context.Members
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);
    }

    public async Task<bool> ExistsAsync(int boardId, int userId)
    {
        return await _context.Members
            .AnyAsync(m => m.BoardId == boardId && m.UserId == userId);
    }

    public async System.Threading.Tasks.Task AddAsync(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(Member member)
    {
        _context.Members.Remove(member);
        await _context.SaveChangesAsync();
    }
}