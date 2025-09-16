using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Application.Utils;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICheckAccessService _checkAccessService;
    private readonly ICommentService _commentService;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly string _attachmentsPath;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        ITaskRepository taskRepository,
        ICheckAccessService checkAccessService,
        ICommentService commentService,
        IUserContext userContext,
        IMapper mapper)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _checkAccessService = checkAccessService;
        _commentService = commentService;
        _userContext = userContext;
        _mapper = mapper;
        _attachmentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");

        if (!Directory.Exists(_attachmentsPath))
            Directory.CreateDirectory(_attachmentsPath);
    }

    public async Task<AttachmentResponseDto> AddAttachmentAsync(int taskId, IFormFile file, int userId)
    {
        var task = await _taskRepository.GetByIdWithTaskListAsync(taskId);
        if (task == null)
            throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess)
            throw new ForbiddenAccessException();

        ValidateFile(file);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
        var filePath = Path.Combine(_attachmentsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

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

        await _attachmentRepository.AddAsync(attachment);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.AddedAttachment(_userContext.GetCurrentUserName(), file.FileName),
            userId
        );

        var createdAttachment = await _attachmentRepository.GetByIdAsync(attachment.Id);
        if (createdAttachment == null) throw new NotFoundException("Attachment not found after creation");

        return _mapper.Map<AttachmentResponseDto>(createdAttachment);
    }

    public async System.Threading.Tasks.Task RemoveAttachmentAsync(int taskId, int attachmentId, int userId)
    {
        var task = await _taskRepository.GetByIdWithTaskListAsync(taskId);
        if (task == null)
            throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess)
            throw new ForbiddenAccessException();

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null || attachment.TaskId != taskId)
            throw new NotFoundException("Attachment not found");

        var filePath = Path.Combine(_attachmentsPath, attachment.FilePath);
        if (File.Exists(filePath))
            File.Delete(filePath);

        await _attachmentRepository.DeleteAsync(attachment);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.RemovedAttachment(_userContext.GetCurrentUserName(), attachment.FileName),
            userId
        );
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("File is required");

        if (file.Length > 10 * 1024 * 1024)
            throw new ValidationException("File size cannot exceed 10MB");

        var allowedExtensions = new[] {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".txt", ".zip", ".rar"
        };

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            throw new ValidationException("Invalid file type");
    }
}