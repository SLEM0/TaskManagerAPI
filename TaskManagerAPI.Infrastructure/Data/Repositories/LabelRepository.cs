using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data.Repositories;

public class LabelRepository : ILabelRepository
{
    private readonly AppDbContext _context;

    public LabelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Label?> GetByIdAsync(int id)
    {
        return await _context.Labels.FindAsync(id);
    }

    public async Task<Label?> GetByIdWithBoardAsync(int id)
    {
        return await _context.Labels
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async System.Threading.Tasks.Task AddAsync(Label label)
    {
        await _context.Labels.AddAsync(label);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateAsync(Label label)
    {
        _context.Labels.Update(label);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(Label label)
    {
        _context.Labels.Remove(label);
        await _context.SaveChangesAsync();
    }
}