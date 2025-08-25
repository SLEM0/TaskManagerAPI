using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class TaskListService : ITaskListService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _authService;

    public TaskListService(AppDbContext context, ICheckAccessService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<TaskListResponseDto> GetTaskListDetailsAsync(int taskListId)
    {
        var taskList = await _context.TaskLists
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Labels) // ← Добавляем загрузку меток!
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.User)
            .Include(tl => tl.Board)
            .AsSplitQuery() // ← Важно для избежания cartesian product
            .FirstOrDefaultAsync(tl => tl.Id == taskListId);

        if (taskList == null) throw new KeyNotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new UnauthorizedAccessException();

        return new TaskListResponseDto
        {
            Id = taskList.Id,
            Title = taskList.Title,
            BoardId = taskList.BoardId,
            CreatedAt = taskList.CreatedAt,
            Order = taskList.Order,
            Tasks = taskList.Tasks
                .OrderBy(t => t.Order)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    TaskListId = t.TaskListId,
                    Order = t.Order,
                    Labels = t.Labels.Select(label => new LabelResponseDto // ← Добавляем метки!
                    {
                        Id = label.Id,
                        Name = label.Name,
                        Color = label.Color,
                        CreatedAt = label.CreatedAt,
                        BoardId = label.BoardId
                    }),
                    Members = t.Members.Select(member => new MemberResponseDto
                    {
                        Id = member.Id,
                        BoardId = member.BoardId,
                        UserId = member.User.Id,
                        UserName = member.User.Username,
                        UserEmail = member.User.Email,
                        Role = member.Role,
                        AddedAt = member.AddedAt
                    })
                })
        };
    }

    public async Task<TaskListResponseDto> CreateTaskListAsync(TaskListRequestDto taskListDto, int boardId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var lastOrder = await _context.TaskLists
        .Where(tl => tl.BoardId == boardId)
        .OrderByDescending(tl => tl.Order)
        .Select(tl => tl.Order)
        .FirstOrDefaultAsync();

        var taskList = new TaskList
        {
            Title = taskListDto.Title,
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow,
            Order = lastOrder + 1
        };

        _context.TaskLists.Add(taskList);
        await _context.SaveChangesAsync();

        return new TaskListResponseDto
        {
            Id = taskList.Id,
            Title = taskList.Title,
            BoardId = taskList.BoardId,
            CreatedAt = taskList.CreatedAt,
            Order = taskList.Order,
            Tasks = new List<TaskResponseDto>()
        };
    }

    public async Task<TaskListResponseDto> UpdateTaskListAsync(int taskListId, TaskListRequestDto taskListDto)
    {
        var taskList = await _context.TaskLists
            .Include(tl => tl.Board)
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Labels) // ← Добавляем загрузку меток!
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(tl => tl.Id == taskListId);

        if (taskList == null) throw new KeyNotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        taskList.Title = taskListDto.Title;
        await _context.SaveChangesAsync();

        return new TaskListResponseDto
        {
            Id = taskList.Id,
            Title = taskList.Title,
            BoardId = taskList.BoardId,
            CreatedAt = taskList.CreatedAt,
            Order = taskList.Order,
            Tasks = taskList.Tasks
                .OrderBy(t => t.Order)
                .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                TaskListId = t.TaskListId,
                Order = t.Order,
                Labels = t.Labels.Select(label => new LabelResponseDto // ← Добавляем метки!
                {
                    Id = label.Id,
                    Name = label.Name,
                    Color = label.Color,
                    CreatedAt = label.CreatedAt,
                    BoardId = label.BoardId
                }),
                Members = t.Members.Select(member => new MemberResponseDto
                {
                    Id = member.Id,
                    BoardId = member.BoardId,
                    UserId = member.User.Id,
                    UserName = member.User.Username,
                    UserEmail = member.User.Email,
                    Role = member.Role,
                    AddedAt = member.AddedAt
                })
                })
        };
    }

    public async System.Threading.Tasks.Task DeleteTaskListAsync(int taskListId)
    {
        var taskList = await _context.TaskLists
            .Include(tl => tl.Board)
            .FirstOrDefaultAsync(tl => tl.Id == taskListId);

        if (taskList == null) throw new KeyNotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        _context.TaskLists.Remove(taskList);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskListResponseDto> MoveTaskListAsync(int taskListId, MoveTaskListRequestDto moveDto)
    {
        var taskList = await _context.TaskLists
            .Include(tl => tl.Board)
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Labels)
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(tl => tl.Id == taskListId);

        if (taskList == null) throw new KeyNotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        // Получаем все списки в доске (кроме перемещаемого)
        var listsInBoard = await _context.TaskLists
            .Where(tl => tl.BoardId == taskList.BoardId && tl.Id != taskListId)
            .OrderBy(tl => tl.Order)
            .ToListAsync();

        // Вставляем список на нужную позицию
        listsInBoard.Insert(moveDto.NewOrder, taskList);

        // Пересчитываем порядок для всех списков в доске
        for (int i = 0; i < listsInBoard.Count; i++)
        {
            listsInBoard[i].Order = i + 1; // Порядок начинается с 1
        }

        await _context.SaveChangesAsync();

        return new TaskListResponseDto
        {
            Id = taskList.Id,
            Title = taskList.Title,
            BoardId = taskList.BoardId,
            CreatedAt = taskList.CreatedAt,
            Order = taskList.Order,
            Tasks = taskList.Tasks
                .OrderBy(t => t.Order)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    TaskListId = t.TaskListId,
                    Order = t.Order,
                    Labels = t.Labels.Select(label => new LabelResponseDto
                    {
                        Id = label.Id,
                        Name = label.Name,
                        Color = label.Color,
                        CreatedAt = label.CreatedAt,
                        BoardId = label.BoardId
                    }),
                    Members = t.Members.Select(member => new MemberResponseDto
                    {
                        Id = member.Id,
                        BoardId = member.BoardId,
                        UserId = member.User.Id,
                        UserName = member.User.Username,
                        UserEmail = member.User.Email,
                        Role = member.Role,
                        AddedAt = member.AddedAt
                    })
                })
        };
    }
}