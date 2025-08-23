using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _authService;

    public TaskService(AppDbContext context, ICheckAccessService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto taskDto, int listId)
    {
        // Проверяем доступ к списку задач (только редакторы и владельцы)
        var (hasAccess, _) = await _authService.CheckTaskListAccessAsync(listId, BoardRole.Editor);
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
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Order = lastOrder + 1
        };

        _context.Tasks.Add(task);
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
            Labels = new List<LabelResponseDto>()
        };
    }

    public async Task<TaskResponseDto> GetTaskDetailsAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.TaskList)
            .ThenInclude(tl => tl.Board)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        // Проверяем доступ к доске
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Viewer);
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
            })
        };
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto taskDto)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        task.Title = taskDto.Title;
        task.Description = taskDto.Description;
        task.DueDate = taskDto.DueDate;
        task.IsCompleted = taskDto.IsCompleted ?? task.IsCompleted;

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
            })
        };
    }

    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskResponseDto> MoveTaskAsync(int taskId, MoveTaskRequestDto moveDto)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        // Загружаем исходный список и проверяем его существование
        var sourceList = await _context.TaskLists
            .FirstOrDefaultAsync(tl => tl.Id == task.TaskListId);

        if (sourceList == null) throw new KeyNotFoundException("Source task list not found");

        // Проверяем доступ к исходной доске
        var (hasSourceAccess, _) = await _authService.CheckBoardAccessAsync(sourceList.BoardId, BoardRole.Editor);
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
        var (hasTargetAccess, _) = await _authService.CheckBoardAccessAsync(targetList.BoardId, BoardRole.Editor);
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
            })
        };
    }

    public async Task<TaskResponseDto> AddLabelToTaskAsync(int taskId, AddLabelToTaskRequestDto addLabelDto)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var label = await _context.Labels.FindAsync(addLabelDto.LabelId);
        if (label == null) throw new KeyNotFoundException("Label not found");

        if (task.Labels.Any(l => l.Id == addLabelDto.LabelId))
            throw new InvalidOperationException("Label already added to task");

        task.Labels.Add(label);
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
            })
        };
    }

    public async Task<TaskResponseDto> RemoveLabelFromTaskAsync(int taskId, int labelId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .Include(t => t.TaskList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) throw new KeyNotFoundException("Task not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var label = task.Labels.FirstOrDefault(l => l.Id == labelId);
        if (label == null) throw new KeyNotFoundException("Label not found on this task");

        task.Labels.Remove(label);
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
            })
        };
    }
}