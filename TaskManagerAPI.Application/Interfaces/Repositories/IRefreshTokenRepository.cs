using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
    System.Threading.Tasks.Task AddAsync(RefreshToken refreshToken);
    System.Threading.Tasks.Task DeleteAsync(RefreshToken refreshToken);
    System.Threading.Tasks.Task DeleteRangeAsync(IEnumerable<RefreshToken> refreshTokens);
}