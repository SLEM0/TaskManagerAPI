using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;

namespace TaskManagerAPI.Application.Interfaces;

public interface ITaskListService
{
    Task<TaskListResponseDto> GetTaskListDetailsAsync(int taskListId);
    Task<TaskListResponseDto> CreateTaskListAsync(TaskListRequestDto taskListDto, int boardId);
    Task<TaskListResponseDto> UpdateTaskListAsync(int taskListId, TaskListRequestDto taskListDto);
    Task DeleteTaskListAsync(int taskListId);
    Task<TaskListResponseDto> MoveTaskListAsync(int taskListId, MoveTaskListRequestDto moveDto);
}