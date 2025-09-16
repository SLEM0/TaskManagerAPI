using AutoMapper;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Application.Utils;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskListRepository _taskListRepository;
    private readonly ILabelRepository _labelRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICheckAccessService _checkAccessService;
    private readonly ICommentService _commentService;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;

    public TaskService(
        ITaskRepository taskRepository,
        ITaskListRepository taskListRepository,
        ILabelRepository labelRepository,
        IMemberRepository memberRepository,
        ICheckAccessService checkAccessService,
        ICommentService commentService,
        IUserContext userContext,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _taskListRepository = taskListRepository;
        _labelRepository = labelRepository;
        _memberRepository = memberRepository;
        _checkAccessService = checkAccessService;
        _commentService = commentService;
        _userContext = userContext;
        _mapper = mapper;
    }

    public async Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto taskDto, int listId)
    {
        var taskList = await _taskListRepository.GetByIdWithBoardAsync(listId);
        if (taskList == null) throw new NotFoundException("Task list not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var lastOrder = await _taskRepository.GetMaxOrderAsync(listId);

        var task = new Domain.Entities.Task
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            DueDate = taskDto.DueDate,
            TaskListId = listId,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Order = lastOrder + 1,
            Labels = new List<Label>(),
            Members = new List<Member>(),
            Comments = new List<Comment>(),
            Attachments = new List<Attachment>()
        };

        await _taskRepository.AddAsync(task);

        await _commentService.SystemLogActionAsync(
            task.Id,
            SystemMessages.CreatedTask(_userContext.GetCurrentUserName()),
            _userContext.GetCurrentUserId()
        );

        var createdTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
        if (createdTask == null) throw new NotFoundException("Task not found after creation");

        return _mapper.Map<TaskResponseDto>(createdTask);
    }

    public async Task<TaskResponseDto> GetTaskDetailsAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new ForbiddenAccessException();

        return _mapper.Map<TaskResponseDto>(task);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto taskDto)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

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

        if (changes.Any())
        {
            foreach (string change in changes)
            {
                await _commentService.SystemLogActionAsync(taskId, change, _userContext.GetCurrentUserId());
            }
        }

        await _taskRepository.UpdateAsync(task);

        var updatedTask = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (updatedTask == null) throw new NotFoundException("Task not found after update");

        return _mapper.Map<TaskResponseDto>(updatedTask);
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdWithTaskListAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        await _taskRepository.DeleteAsync(task);
    }

    public async Task<TaskResponseDto> MoveTaskAsync(int taskId, MoveTaskRequestDto moveDto)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var sourceList = await _taskListRepository.GetByIdAsync(task.TaskListId);
        if (sourceList == null) throw new NotFoundException("Source task list not found");

        var (hasSourceAccess, _) = await _checkAccessService.CheckBoardAccessAsync(sourceList.BoardId, BoardRole.Editor);
        if (!hasSourceAccess) throw new ForbiddenAccessException();

        var targetList = await _taskListRepository.GetByIdAsync(moveDto.NewListId);
        if (targetList == null) throw new NotFoundException("Target task list not found");

        if (sourceList.BoardId != targetList.BoardId)
        {
            throw new ValidationException("Moving tasks between different boards is not allowed");
        }

        var (hasTargetAccess, _) = await _checkAccessService.CheckBoardAccessAsync(targetList.BoardId, BoardRole.Editor);
        if (!hasTargetAccess) throw new ForbiddenAccessException();

        var tasksInTargetList = (await _taskRepository.GetByTaskListIdAsync(moveDto.NewListId))
            .Where(t => t.Id != taskId)
            .ToList();

        if (moveDto.NewOrder < 0 || moveDto.NewOrder > tasksInTargetList.Count)
        {
            throw new ValidationException("New order position is out of range");
        }

        tasksInTargetList.Insert(moveDto.NewOrder, task);

        for (int i = 0; i < tasksInTargetList.Count; i++)
        {
            tasksInTargetList[i].Order = i + 1;
        }

        if (moveDto.NewListId != task.TaskListId)
        {
            task.TaskListId = moveDto.NewListId;
            await _commentService.SystemLogActionAsync(
                taskId,
                SystemMessages.MovedToList(_userContext.GetCurrentUserName(), targetList.Title),
                _userContext.GetCurrentUserId()
            );
        }

        await _taskRepository.UpdateRangeAsync(tasksInTargetList);

        var movedTask = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (movedTask == null) throw new NotFoundException("Task not found after moving");

        return _mapper.Map<TaskResponseDto>(movedTask);
    }

    public async Task<TaskResponseDto> AddLabelToTaskAsync(int taskId, int labelId)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var label = await _labelRepository.GetByIdAsync(labelId);
        if (label == null) throw new NotFoundException("Label not found");

        if (task.Labels.Any(l => l.Id == labelId))
            throw new ValidationException("Label already added to task");

        task.Labels.Add(label);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.AddedLabel(_userContext.GetCurrentUserName(), label.Name),
            _userContext.GetCurrentUserId()
        );

        await _taskRepository.UpdateAsync(task);

        var updatedTask = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (updatedTask == null) throw new NotFoundException("Task not found after adding label");

        return _mapper.Map<TaskResponseDto>(updatedTask);
    }

    public async Task<TaskResponseDto> RemoveLabelFromTaskAsync(int taskId, int labelId)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(task.TaskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var label = task.Labels.FirstOrDefault(l => l.Id == labelId);
        if (label == null) throw new NotFoundException("Label not found on this task");

        task.Labels.Remove(label);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.RemovedLabel(_userContext.GetCurrentUserName(), label.Name),
            _userContext.GetCurrentUserId()
        );

        await _taskRepository.UpdateAsync(task);

        var updatedTask = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (updatedTask == null) throw new NotFoundException("Task not found after removing label");

        return _mapper.Map<TaskResponseDto>(updatedTask);
    }

    public async Task<TaskResponseDto> AssignTaskAsync(int taskId, int userId)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var taskList = await _taskListRepository.GetByIdAsync(task.TaskListId);
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var boardUser = await _memberRepository.GetByBoardAndUserIdAsync(taskList.BoardId, userId);
        if (boardUser == null)
            throw new NotFoundException("Board member not found or does not belong to this board");

        if (task.Members.Any(a => a.UserId == userId))
            throw new ValidationException("User is already assigned to this task");

        task.Members.Add(boardUser);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.AssignedUser(_userContext.GetCurrentUserName(), boardUser.User.Username),
            _userContext.GetCurrentUserId()
        );

        await _taskRepository.UpdateAsync(task);

        var updatedTask = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (updatedTask == null) throw new NotFoundException("Task not found after assigning user");

        return _mapper.Map<TaskResponseDto>(updatedTask);
    }

    public async Task<TaskResponseDto> UnassignTaskAsync(int taskId, int userId)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null) throw new NotFoundException("Task not found");

        var taskList = await _taskListRepository.GetByIdAsync(task.TaskListId);
        var (hasAccess, _) = await _checkAccessService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var member = task.Members.FirstOrDefault(a => a.UserId == userId);
        if (member == null) throw new NotFoundException("Assignee not found");

        task.Members.Remove(member);

        await _commentService.SystemLogActionAsync(
            taskId,
            SystemMessages.UnassignedUser(_userContext.GetCurrentUserName(), member.User.Username),
            _userContext.GetCurrentUserId()
        );

        await _taskRepository.UpdateAsync(task);

        var updatedTask = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (updatedTask == null) throw new NotFoundException("Task not found after unassigning user");

        return _mapper.Map<TaskResponseDto>(updatedTask);
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetTasksDueBetweenAsync(DateTime start, DateTime end)
    {
        return await _taskRepository.GetTasksDueBetweenAsync(start, end);
    }

    public async System.Threading.Tasks.Task MarkDueDateNotificationSentAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null)
        {
            task.DueDateNotificationSent = true;
            await _taskRepository.UpdateAsync(task);
        }
    }
}