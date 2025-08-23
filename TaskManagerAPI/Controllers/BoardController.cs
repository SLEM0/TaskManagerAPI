using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/boards")]
[Authorize]
public class BoardController : ControllerBase
{
    private readonly IBoardService _boardService;
    private readonly ILabelService _labelService;
    private readonly ITaskListService _taskListService;
    private readonly IUserContext _userContext;

    public BoardController(
        IBoardService boardsService, 
        ILabelService labelService, 
        ITaskListService taskListService, 
        IUserContext userContext
        )
    {
        _boardService = boardsService;
        _labelService = labelService;
        _taskListService = taskListService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShortBoardResponseDto>>> GetAllBoards()
    {
        var userId = _userContext.GetCurrentUserId();
        var boards = await _boardService.GetUserBoardsAsync(userId);
        return Ok(boards);
    }

    [HttpGet("{boardId}")]
    public async Task<ActionResult<BoardResponseDto>> GetBoard(int boardId)
    {
        try
        {
            var board = await _boardService.GetBoardDetailsAsync(boardId);
            return Ok(board);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<BoardResponseDto>> CreateBoard([FromBody] BoardRequestDto boardDto)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var board = await _boardService.CreateBoardAsync(boardDto, userId);
            return CreatedAtAction(nameof(GetBoard), new { boardid = board.Id }, board);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{boardId}")]
    public async Task<ActionResult<BoardResponseDto>> UpdateBoard(int boardId, [FromBody] BoardRequestDto boardDto)
    {
        try
        {
            var board = await _boardService.UpdateBoardAsync(boardId, boardDto);
            return Ok(board);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{boardId}")]
    public async Task<IActionResult> DeleteBoard(int boardId)
    {
        try
        {
            await _boardService.DeleteBoardAsync(boardId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{boardId}/members")]
    public async Task<ActionResult<MemberResponseDto>> AddBoardMember(int boardId, [FromBody] MemberRequestDto dto)
    {
        try
        {
            var result = await _boardService.AddBoardMemberAsync(boardId, dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{boardId}/members")]
    public async Task<IActionResult> RemoveBoardMember(int boardId, [FromBody] RemoveMemberRequestDto removeMemberDto)
    {
        try
        {
            var requestingUserId = _userContext.GetCurrentUserId();
            await _boardService.RemoveBoardMemberAsync(boardId, removeMemberDto.UserId, requestingUserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{boardId}/tasklists")]
    public async Task<ActionResult<IEnumerable<TaskListResponseDto>>> GetBoardTaskLists(int boardId, [FromQuery] TaskFilterDto filterDto)
    {
        try
        {
            var result = await _boardService.GetFilteredBoardTasksAsync(boardId, filterDto);
            return Ok(result);
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

    [HttpPost("{boardId}/labels")]
    public async Task<ActionResult<LabelResponseDto>> CreateLabel(int boardId, [FromBody] LabelRequestDto labelDto)
    {
        try
        {
            var label = await _labelService.CreateLabelAsync(labelDto, boardId);

            return CreatedAtAction(
                actionName: nameof(LabelController.GetLabel), // Метод в другом контроллере
                controllerName: "Label", // Важно: имя контроллера без суффикса
                routeValues: new { labelId = label.Id }, // Новые параметры пути (только labelId)
                value: label
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{boardId}/lists")]
    public async Task<ActionResult<TaskListResponseDto>> CreateTaskList(int boardId, [FromBody] TaskListRequestDto taskListDto)
    {
        try
        {
            var taskList = await _taskListService.CreateTaskListAsync(taskListDto, boardId);
            return CreatedAtAction(
                actionName: nameof(TaskListController.GetTaskList), // Метод в другом контроллере
                controllerName: "TaskList", // Важно: имя контроллера без суффикса
                routeValues: new { listId = taskList.Id }, // Новые параметры пути (только labelId)
                value: taskList
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
