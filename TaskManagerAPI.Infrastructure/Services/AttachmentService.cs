using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Application.Utils;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _checkAccessService;
    private readonly ICommentService _commentService;
    private readonly string _attachmentsPath;
    private readonly IUserContext _userContext;

    public AttachmentService(
        AppDbContext context, 
        ICheckAccessService checkAccessService, 
        ICommentService commentService, 
        IUserContext userContext
        )
    {
        _context = context;
        _checkAccessService = checkAccessService;
        _commentService = commentService;
        _userContext = userContext;
        _attachmentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");

        // Создаем папку для вложений если ее нет
        if (!Directory.Exists(_attachmentsPath))
            Directory.CreateDirectory(_attachmentsPath);
    }

    public async Task<AttachmentResponseDto> AddAttachmentAsync(int taskId, IFormFile file, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new KeyNotFoundException("Task not found");

        // Проверяем доступ к доске
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess)
            throw new UnauthorizedAccessException();

        // Валидация файла
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
            throw new ArgumentException("File size cannot exceed 10MB");

        var allowedExtensions = new[] {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".txt", ".zip", ".rar"
        };

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException("Invalid file type");

        // Сохранение файла на диск
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_attachmentsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Создание записи в БД
        var attachment = new Attachment
        {
            FileName = file.FileName,
            FilePath = fileName,
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow,
            TaskId = taskId,
            UploadedById = userId
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        // Логирование действия
        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.AddedAttachment(_userContext.GetCurrentUserName(), file.FileName),
            userId
        );

        return new AttachmentResponseDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FileSize = attachment.FileSize,
            ContentType = attachment.ContentType,
            UploadedAt = attachment.UploadedAt,
            UploadedById = attachment.UploadedById,
            FileUrl = $"/attachments/{attachment.FilePath}"
        };
    }

    public async System.Threading.Tasks.Task RemoveAttachmentAsync(int taskId, int attachmentId, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new KeyNotFoundException("Task not found");

        // Проверяем доступ к доске
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess)
            throw new UnauthorizedAccessException();

        var attachment = await _context.Attachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == taskId);

        if (attachment == null)
            throw new KeyNotFoundException("Attachment not found");

        // Удаление файла с диска
        var filePath = Path.Combine(_attachmentsPath, attachment.FilePath);
        if (File.Exists(filePath))
            File.Delete(filePath);

        // Удаление записи из БД
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        // Логирование действия
        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.RemovedAttachment(_userContext.GetCurrentUserName(), attachment.FileName),
            userId
        );
    }
}