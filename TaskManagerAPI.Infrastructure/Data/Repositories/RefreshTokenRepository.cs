using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.Expires > DateTime.UtcNow);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteRangeAsync(IEnumerable<RefreshToken> refreshTokens)
    {
        _context.RefreshTokens.RemoveRange(refreshTokens);
        await _context.SaveChangesAsync();
    }
}