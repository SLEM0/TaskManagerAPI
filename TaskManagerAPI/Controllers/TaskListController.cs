using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskListController : ControllerBase
{
    private readonly ITaskListService _taskListService;
    private readonly ITaskService _taskService;

    public TaskListController(ITaskListService taskListService, ITaskService taskService)
    {
        _taskListService = taskListService;
        _taskService = taskService;
    }

    [HttpGet("{listId}")]
    public async Task<ActionResult<TaskListResponseDto>> GetTaskList(int listId)
    {
        try
        {
            var taskList = await _taskListService.GetTaskListDetailsAsync(listId);
            return Ok(taskList);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{listId}")]
    public async Task<ActionResult<TaskListResponseDto>> UpdateTaskList(int listId, [FromBody] TaskListRequestDto taskListDto)
    {
        try
        {
            var taskList = await _taskListService.UpdateTaskListAsync(listId, taskListDto);
            return Ok(taskList);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{listId}")]
    public async Task<IActionResult> DeleteTaskList(int listId)
    {
        try
        {
            await _taskListService.DeleteTaskListAsync(listId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{listId}/move")]
    public async Task<ActionResult<TaskListResponseDto>> MoveTaskList(int listId, [FromBody] MoveTaskListRequestDto moveDto)
    {
        try
        {
            var task = await _taskListService.MoveTaskListAsync(listId, moveDto);
            return Ok(task);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{listId}/tasks")]
    public async Task<ActionResult<ShortTaskResponseDto>> CreateTask(int listId, [FromBody] TaskRequestDto taskDto)
    {
        try
        {
            var task = await _taskService.CreateTaskAsync(taskDto, listId);
            return CreatedAtAction(
                actionName: nameof(TaskController.GetTask), // Метод в другом контроллере
                controllerName: "Task", // Важно: имя контроллера без суффикса
                routeValues: new { taskId = task.Id }, // Новые параметры пути (только labelId)
                value: task
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}