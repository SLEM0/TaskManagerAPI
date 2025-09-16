using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username &&
                         (excludeUserId == null || u.Id != excludeUserId));
    }

    public async System.Threading.Tasks.Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByConfirmationCodeAsync(int code)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.EmailConfirmationCode == code &&
                                     u.EmailConfirmationCodeExpires > DateTime.UtcNow);
    }
}