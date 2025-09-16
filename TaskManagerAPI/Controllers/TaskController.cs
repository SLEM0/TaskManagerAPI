using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Interfaces.Services;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int taskId)
    {
        var task = await _taskService.GetTaskDetailsAsync(taskId);
        return Ok(task);
    }

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int taskId, [FromBody] TaskRequestDto taskDto)
    {
        var task = await _taskService.UpdateTaskAsync(taskId, taskDto);
        return Ok(task);
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        return NoContent();
    }

    [HttpPut("{taskId}/move")]
    public async Task<ActionResult<TaskResponseDto>> MoveTask(int taskId, [FromBody] MoveTaskRequestDto moveDto)
    {
        var task = await _taskService.MoveTaskAsync(taskId, moveDto);
        return Ok(task);
    }

    [HttpPost("{taskId}/labels/{labelId}")]
    public async Task<ActionResult<TaskResponseDto>> AddLabelToTask(int taskId, int labelId)
    {
        var task = await _taskService.AddLabelToTaskAsync(taskId, labelId);
        return Ok(task);
    }

    [HttpDelete("{taskId}/labels/{labelId}")]
    public async Task<ActionResult<TaskResponseDto>> RemoveLabelFromTask(int taskId, int labelId)
    {
        var task = await _taskService.RemoveLabelFromTaskAsync(taskId, labelId);
        return Ok(task);
    }

    [HttpPost("{taskId}/assignees/{userId}")]
    public async Task<ActionResult<TaskResponseDto>> AssignTask(int taskId, int userId)
    {
        var task = await _taskService.AssignTaskAsync(taskId, userId);
        return Ok(task);
    }

    [HttpDelete("{taskId}/assignees/{userId}")]
    public async Task<ActionResult<TaskResponseDto>> UnassignTask(int taskId, int userId)
    {
        var task = await _taskService.UnassignTaskAsync(taskId, userId);
        return Ok(task);
    }
}