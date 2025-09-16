using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(int id);
    Task<Attachment?> GetByIdWithTaskAsync(int id);
    System.Threading.Tasks.Task AddAsync(Attachment attachment);
    System.Threading.Tasks.Task DeleteAsync(Attachment attachment);
}