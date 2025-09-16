using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface ITaskService
{
    Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto taskDto, int listId);
    Task<TaskResponseDto> GetTaskDetailsAsync(int taskId);
    Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto taskDto);
    System.Threading.Tasks.Task DeleteTaskAsync(int taskId);
    Task<TaskResponseDto> MoveTaskAsync(int taskId, MoveTaskRequestDto moveDto);
    Task<TaskResponseDto> AddLabelToTaskAsync(int taskId, int labelId);
    Task<TaskResponseDto> RemoveLabelFromTaskAsync(int taskId, int labelId);
    Task<TaskResponseDto> AssignTaskAsync(int taskId, int userId);
    Task<TaskResponseDto> UnassignTaskAsync(int taskId, int userId);
    Task<IEnumerable<Domain.Entities.Task>> GetTasksDueBetweenAsync(DateTime start, DateTime end);
    System.Threading.Tasks.Task MarkDueDateNotificationSentAsync(int taskId);
}