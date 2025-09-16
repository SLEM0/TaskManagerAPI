using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface ILabelRepository
{
    Task<Label?> GetByIdAsync(int id);
    Task<Label?> GetByIdWithBoardAsync(int id);
    System.Threading.Tasks.Task AddAsync(Label label);
    System.Threading.Tasks.Task UpdateAsync(Label label);
    System.Threading.Tasks.Task DeleteAsync(Label label);
}