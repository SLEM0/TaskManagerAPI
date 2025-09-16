using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<Comment?> GetByIdWithAuthorAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}