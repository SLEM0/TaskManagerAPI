using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Application.Utils;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _checkAccessService;
    private readonly ICommentService _commentService;
    private readonly IUserContext _userContext;

    public TaskService(AppDbContext context, ICheckAccessService checkAccessService, ICommentService commentService, IUserContext userContext)
    {
        _context = context;
        _checkAccessService = checkAccessService;
        _commentService = commentService;
        _userContext = userContext;
    }

    public async Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto taskDto, int listId)
    {
        // ПРОВЕРЯЕМ существование списка задач
        var taskList = await _context.TaskLists
            .Include(tl => tl.Board) // Для проверки доступа
            .FirstOrDefaultAsync(tl => tl.Id == listId);

        if (taskList == null)
            throw new KeyNotFoundException("Task list not found");

        // Проверяем доступ к доске (а не к списку)
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        // Определяем порядок - последняя задача в списке + 1
        var lastOrder = await _context.Tasks
            .Where(t => t.TaskListId == listId)
            .OrderByDescending(t => t.Order)
            .Select(t => t.Order)
            .FirstOrDefaultAsync();

        var task = new Domain.Entities.Task
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            DueDate = taskDto.DueDate,
            TaskListId = listId,
            TaskList = taskList, // ← ВАЖНО: устанавливаем навигационное свойство!
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Order = lastOrder + 1,
            Labels = new List<Label>(),
            Members = new List<Member>(),
            Comments = new List<Comment>(),
            Attachments = new List<Attachment>()
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        await _commentService.SystemLogActionAsync(
            task.Id,
            SystemMessages.CreatedTask(_userContext.GetCurrentUserName()),
            _userContext.GetCurrentUserId()
        );

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = new List<LabelResponseDto>(),
            Members = new List<MemberResponseDto>(),
            Comments = new List<CommentResponseDto>(),
            Attachments = new List<AttachmentResponseDto>()
        };
    }

    public async Task<TaskResponseDto> GetTaskDetailsAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.TaskList)
            .ThenInclude(tl => tl.Board)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        // Проверяем доступ к доске
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new UnauthorizedAccessException();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                CreatedAt = l.CreatedAt,
                BoardId = l.BoardId,
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto taskDto)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var changes = new List<string>();
        var userName = _userContext.GetCurrentUserName();

        if (task.Title != taskDto.Title)
        {
            changes.Add(SystemMessages.ChangedTitle(userName, taskDto.Title));
            task.Title = taskDto.Title;
        }

        if (task.Description != taskDto.Description)
        {
            changes.Add(SystemMessages.ChangedDescription(userName));
            task.Description = taskDto.Description;
        }

        if (task.DueDate != taskDto.DueDate)
        {
            changes.Add(SystemMessages.ChangedDueDate(userName, taskDto.DueDate));
            task.DueDate = taskDto.DueDate;
        }

        if (taskDto.IsCompleted.HasValue && task.IsCompleted != taskDto.IsCompleted)
        {
            changes.Add(taskDto.IsCompleted.Value ?
                SystemMessages.MarkedAsCompleted(userName) :
                SystemMessages.MarkedAsIncomplete(userName));
            task.IsCompleted = taskDto.IsCompleted.Value;
        }

        // Логируем каждое изменение отдельным комментарием
        if (changes.Any())
        {
            foreach (string change in changes)
            {
                await _commentService.SystemLogActionAsync(taskId, change, _userContext.GetCurrentUserId());
            }
        }

        await _context.SaveChangesAsync();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                CreatedAt = l.CreatedAt,
                BoardId = l.BoardId
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskResponseDto> MoveTaskAsync(int taskId, MoveTaskRequestDto moveDto)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        // Загружаем исходный список и проверяем его существование
        var sourceList = await _context.TaskLists
            .FirstOrDefaultAsync(tl => tl.Id == task.TaskListId);

        if (sourceList == null) throw new KeyNotFoundException("Source task list not found");

        // Проверяем доступ к исходной доске
        var (hasSourceAccess, _) = await _checkAccessService.CheckBoardAccessAsync(sourceList.BoardId, BoardRole.Editor);
        if (!hasSourceAccess) throw new UnauthorizedAccessException();

        // Загружаем целевой список и проверяем его существование
        var targetList = await _context.TaskLists
            .FirstOrDefaultAsync(tl => tl.Id == moveDto.NewListId);

        if (targetList == null) throw new KeyNotFoundException("Target task list not found");

        // ⚠️ ЗАПРЕЩАЕМ перемещение между досками
        if (sourceList.BoardId != targetList.BoardId)
        {
            throw new InvalidOperationException("Moving tasks between different boards is not allowed");
        }

        // Проверяем доступ к целевой доске (та же самая доска, но для consistency)
        var (hasTargetAccess, _) = await _checkAccessService.CheckBoardAccessAsync(targetList.BoardId, BoardRole.Editor);
        if (!hasTargetAccess) throw new UnauthorizedAccessException();

        // Получаем все задачи в целевом списке (кроме перемещаемой)
        var tasksInTargetList = await _context.Tasks
            .Where(t => t.TaskListId == moveDto.NewListId && t.Id != taskId)
            .OrderBy(t => t.Order)
            .ToListAsync();

        // Проверяем валидность новой позиции
        if (moveDto.NewOrder < 0 || moveDto.NewOrder > tasksInTargetList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(moveDto.NewOrder),
                "New order position is out of range");
        }

        // Вставляем задачу на нужную позицию
        tasksInTargetList.Insert(moveDto.NewOrder, task);

        // Пересчитываем порядок для всех задач в списке
        for (int i = 0; i < tasksInTargetList.Count; i++)
        {
            tasksInTargetList[i].Order = i + 1;
        }

        // Обновляем список задачи (если список изменился)
        if (moveDto.NewListId != task.TaskListId)
        {
            task.TaskListId = moveDto.NewListId;
            var newList = await _context.TaskLists.FindAsync(moveDto.NewListId);
            await _commentService.SystemLogActionAsync(
                taskId,
                SystemMessages.MovedToList(_userContext.GetCurrentUserName(), newList.Title),
                _userContext.GetCurrentUserId()
            );
        }

        await _context.SaveChangesAsync();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(label => new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                Color = label.Color,
                CreatedAt = label.CreatedAt,
                BoardId = label.BoardId
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async Task<TaskResponseDto> AddLabelToTaskAsync(int taskId, int labelId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.TaskList)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var label = await _context.Labels.FindAsync(labelId);
        if (label == null) throw new KeyNotFoundException("Label not found");

        if (task.Labels.Any(l => l.Id == labelId))
            throw new InvalidOperationException("Label already added to task");

        task.Labels.Add(label);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.AddedLabel(_userContext.GetCurrentUserName(), label.Name),
            _userContext.GetCurrentUserId()
        );

        await _context.SaveChangesAsync();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                CreatedAt = l.CreatedAt,
                BoardId = l.BoardId
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async Task<TaskResponseDto> RemoveLabelFromTaskAsync(int taskId, int labelId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.TaskList)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var label = task.Labels.FirstOrDefault(l => l.Id == labelId);
        if (label == null) throw new KeyNotFoundException("Label not found on this task");

        task.Labels.Remove(label);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.RemovedLabel(_userContext.GetCurrentUserName(), label.Name),
            _userContext.GetCurrentUserId()
        );

        await _context.SaveChangesAsync();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                CreatedAt = l.CreatedAt,
                BoardId = l.BoardId
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async Task<TaskResponseDto> AssignTaskAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(a => a.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        // Проверяем доступ к доске задачи
        var taskList = await _context.TaskLists.FindAsync(task.TaskListId);
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        // Находим участника доски
        var boardUser = await _context.Members
            .Include(bu => bu.User)
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BoardId == taskList.BoardId);

        if (boardUser == null)
            throw new KeyNotFoundException("Board member not found or does not belong to this board");

        // Проверяем что участник уже не назначен на задачу
        if (task.Members.Any(a => a.Id == userId))
            throw new InvalidOperationException("User is already assigned to this task");

        // Добавляем участника
        task.Members.Add(boardUser);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.AssignedUser(_userContext.GetCurrentUserName(), boardUser.User.Username),
            _userContext.GetCurrentUserId()
        );

        await _context.SaveChangesAsync();

        // Возвращаем DTO с обновленными данными
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(label => new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                Color = label.Color,
                CreatedAt = label.CreatedAt,
                BoardId = label.BoardId
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async Task<TaskResponseDto> UnassignTaskAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.Members)
                .ThenInclude(a => a.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        // Проверяем доступ к доске задачи
        var taskList = await _context.TaskLists.FindAsync(task.TaskListId);
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        // Находим участника для удаления
        var member = task.Members.FirstOrDefault(a => a.UserId == userId);
        if (member == null) throw new KeyNotFoundException("Assignee not found");

        // Удаляем участника
        task.Members.Remove(member);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.UnassignedUser(_userContext.GetCurrentUserName(), member.User.Username),
            _userContext.GetCurrentUserId()
        );

        await _context.SaveChangesAsync();

        // Возвращаем DTO с обновленными данными
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            TaskListId = task.TaskListId,
            Order = task.Order,
            Labels = task.Labels.Select(label => new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                Color = label.Color,
                CreatedAt = label.CreatedAt,
                BoardId = label.BoardId
            }),
            Members = task.Members.Select(member => new MemberResponseDto
            {
                Id = member.Id,
                BoardId = member.BoardId,
                UserId = member.User.Id,
                UserName = member.User.Username,
                UserEmail = member.User.Email,
                Role = member.Role,
                AddedAt = member.AddedAt
            }),
            Comments = task.Comments.Select(comment => new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.Username,
                IsSystemLog = comment.IsSystemLog
            }),
            Attachments = task.Attachments.Select(attachment => new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                FileUrl = $"/attachments/{attachment.FilePath}"
            })
        };
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetTasksDueBetweenAsync(DateTime start, DateTime end)
    {
        return await _context.Tasks
            .Include(t => t.Members)
            .ThenInclude(m => m.User)
            .Where(t => t.DueDate != null &&
                       t.DueDate >= start &&
                       t.DueDate <= end &&
                       !t.IsCompleted &&
                       !t.DueDateNotificationSent)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task MarkDueDateNotificationSentAsync(int taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task != null)
        {
            task.DueDateNotificationSent = true;
            await _context.SaveChangesAsync();
        }
    }
}