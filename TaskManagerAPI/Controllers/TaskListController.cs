using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Interfaces.Services;

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
        var taskList = await _taskListService.GetTaskListDetailsAsync(listId);
        return Ok(taskList);
    }

    [HttpPut("{listId}")]
    public async Task<ActionResult<TaskListResponseDto>> UpdateTaskList(int listId, [FromBody] TaskListRequestDto taskListDto)
    {
        var taskList = await _taskListService.UpdateTaskListAsync(listId, taskListDto);
        return Ok(taskList);
    }

    [HttpDelete("{listId}")]
    public async Task<IActionResult> DeleteTaskList(int listId)
    {
        await _taskListService.DeleteTaskListAsync(listId);
        return NoContent();
    }

    [HttpPut("{listId}/move")]
    public async Task<ActionResult<TaskListResponseDto>> MoveTaskList(int listId, [FromBody] MoveTaskListRequestDto moveDto)
    {
        var task = await _taskListService.MoveTaskListAsync(listId, moveDto);
        return Ok(task);
    }

    [HttpPost("{listId}/tasks")]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(int listId, [FromBody] TaskRequestDto taskDto)
    {
        var task = await _taskService.CreateTaskAsync(taskDto, listId);
        return CreatedAtAction(
            actionName: nameof(TaskController.GetTask),
            controllerName: "Task",
            routeValues: new { taskId = task.Id },
            value: task
        );
    }
}