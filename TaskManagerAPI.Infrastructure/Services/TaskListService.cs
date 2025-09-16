using AutoMapper;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class TaskListService : ITaskListService
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly ICheckAccessService _authService;
    private readonly IMapper _mapper;

    public TaskListService(
        ITaskListRepository taskListRepository,
        ICheckAccessService authService,
        IMapper mapper)
    {
        _taskListRepository = taskListRepository;
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<TaskListResponseDto> GetTaskListDetailsAsync(int taskListId)
    {
        var taskList = await _taskListRepository.GetByIdWithDetailsAsync(taskListId);
        if (taskList == null) throw new NotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new ForbiddenAccessException();

        return _mapper.Map<TaskListResponseDto>(taskList);
    }

    public async Task<TaskListResponseDto> CreateTaskListAsync(TaskListRequestDto taskListDto, int boardId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var lastOrder = await _taskListRepository.GetMaxOrderAsync(boardId);

        var taskList = new TaskList
        {
            Title = taskListDto.Title,
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow,
            Order = lastOrder + 1
        };

        await _taskListRepository.AddAsync(taskList);

        var createdTaskList = await _taskListRepository.GetByIdWithDetailsAsync(taskList.Id);
        if (createdTaskList == null) throw new NotFoundException("Task list not found after creation");

        return _mapper.Map<TaskListResponseDto>(createdTaskList);
    }

    public async Task<TaskListResponseDto> UpdateTaskListAsync(int taskListId, TaskListRequestDto taskListDto)
    {
        var taskList = await _taskListRepository.GetByIdWithDetailsAsync(taskListId);
        if (taskList == null) throw new NotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        taskList.Title = taskListDto.Title;
        await _taskListRepository.UpdateAsync(taskList);

        var updatedTaskList = await _taskListRepository.GetByIdWithDetailsAsync(taskListId);
        if (updatedTaskList == null) throw new NotFoundException("Task list not found after update");

        return _mapper.Map<TaskListResponseDto>(updatedTaskList);
    }

    public async System.Threading.Tasks.Task DeleteTaskListAsync(int taskListId)
    {
        var taskList = await _taskListRepository.GetByIdWithBoardAsync(taskListId);
        if (taskList == null) throw new NotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        await _taskListRepository.DeleteAsync(taskList);
    }

    public async Task<TaskListResponseDto> MoveTaskListAsync(int taskListId, MoveTaskListRequestDto moveDto)
    {
        var taskList = await _taskListRepository.GetByIdWithDetailsAsync(taskListId);
        if (taskList == null) throw new NotFoundException("Task list not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(taskList.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var listsInBoard = (await _taskListRepository.GetByBoardIdAsync(taskList.BoardId))
            .Where(tl => tl.Id != taskListId)
            .ToList();

        if (moveDto.NewOrder < 0 || moveDto.NewOrder > listsInBoard.Count)
        {
            throw new ValidationException("New order position is out of range");
        }

        listsInBoard.Insert(moveDto.NewOrder, taskList);

        for (int i = 0; i < listsInBoard.Count; i++)
        {
            listsInBoard[i].Order = i + 1;
        }

        await _taskListRepository.UpdateRangeAsync(listsInBoard);

        var movedTaskList = await _taskListRepository.GetByIdWithDetailsAsync(taskListId);
        if (movedTaskList == null) throw new NotFoundException("Task list not found after moving");

        return _mapper.Map<TaskListResponseDto>(movedTaskList);
    }
}