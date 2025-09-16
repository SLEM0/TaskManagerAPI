using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly AppDbContext _context;

    public AttachmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Attachment?> GetByIdAsync(int id)
    {
        return await _context.Attachments.FindAsync(id);
    }

    public async Task<Attachment?> GetByIdWithTaskAsync(int id)
    {
        return await _context.Attachments
            .Include(a => a.Task)
            .ThenInclude(t => t.TaskList)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async System.Threading.Tasks.Task AddAsync(Attachment attachment)
    {
        await _context.Attachments.AddAsync(attachment);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(Attachment attachment)
    {
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }
}