using TaskManagerAPI.Application.Dtos.Task;

namespace TaskManagerAPI.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto taskDto, int listId);
    Task<TaskResponseDto> GetTaskDetailsAsync(int taskId);
    Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto taskDto);
    Task DeleteTaskAsync(int taskId);
    Task<TaskResponseDto> MoveTaskAsync(int taskId, MoveTaskRequestDto moveDto);
    Task<TaskResponseDto> AddLabelToTaskAsync(int taskId, AddLabelToTaskRequestDto addLabelDto);
    Task<TaskResponseDto> RemoveLabelFromTaskAsync(int taskId, int labelId);
}