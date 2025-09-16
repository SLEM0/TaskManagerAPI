using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
    System.Threading.Tasks.Task UpdateAsync(User user);
    System.Threading.Tasks.Task AddAsync(User user);
    Task<User?> GetByConfirmationCodeAsync(int code);
}