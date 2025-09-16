using Microsoft.AspNetCore.Http;
using TaskManagerAPI.Application.Dtos.Attachment;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface IAttachmentService
{
    Task<AttachmentResponseDto> AddAttachmentAsync(int taskId, IFormFile file, int userId);
    Task RemoveAttachmentAsync(int taskId, int attachmentId, int userId);
}