using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    System.Threading.Tasks.Task AddAsync(Comment comment);
    Task<Comment?> GetByIdWithAuthorAsync(int id);
}