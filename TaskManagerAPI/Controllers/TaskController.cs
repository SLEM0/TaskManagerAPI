using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Interfaces;

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
        try
        {
            var task = await _taskService.GetTaskDetailsAsync(taskId);
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

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int taskId, [FromBody] TaskRequestDto taskDto)
    {
        try
        {
            var task = await _taskService.UpdateTaskAsync(taskId, taskDto);
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

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        try
        {
            await _taskService.DeleteTaskAsync(taskId);
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

    [HttpPut("{taskId}/move")]
    public async Task<ActionResult<TaskResponseDto>> MoveTask(int taskId, [FromBody] MoveTaskRequestDto moveDto)
    {
        try
        {
            var task = await _taskService.MoveTaskAsync(taskId, moveDto);
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

    [HttpPost("{taskId}/labels")]
    public async Task<ActionResult<TaskResponseDto>> AddLabelToTask(int taskId, [FromBody] AddLabelToTaskRequestDto addLabelDto)
    {
        try
        {
            var task = await _taskService.AddLabelToTaskAsync(taskId, addLabelDto);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{taskId}/labels")]
    public async Task<ActionResult<TaskResponseDto>> RemoveLabelFromTask(int taskId, [FromBody] RemoveLabelRequestDto removeLabelDto)
    {
        try
        {
            var task = await _taskService.RemoveLabelFromTaskAsync(taskId, removeLabelDto.LabelId);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{taskId}/assignees")]
    public async Task<ActionResult<TaskResponseDto>> AssignTask(
        int taskId, [FromBody] MemberRequestDto memberDto)
    {
        try
        {
            var task = await _taskService.AssignTaskAsync(taskId, memberDto);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{taskId}/assignees")]
    public async Task<ActionResult<TaskResponseDto>> UnassignTask(
        int taskId, [FromBody] MemberRequestDto memberDto)
    {
        try
        {
            var task = await _taskService.UnassignTaskAsync(taskId, memberDto);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}