using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Interfaces.Services;

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
        IBoardService boardService,
        ILabelService labelService,
        ITaskListService taskListService,
        IUserContext userContext)
    {
        _boardService = boardService;
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

    [HttpGet("{boardId:int}")]
    public async Task<ActionResult<BoardResponseDto>> GetBoard(int boardId)
    {
        var board = await _boardService.GetBoardDetailsAsync(boardId);
        return Ok(board);
    }

    [HttpPost]
    public async Task<ActionResult<BoardResponseDto>> CreateBoard([FromBody] BoardRequestDto boardDto)
    {
        var userId = _userContext.GetCurrentUserId();
        var board = await _boardService.CreateBoardAsync(boardDto, userId);

        return CreatedAtAction(nameof(GetBoard), new { boardId = board.Id }, board);
    }

    [HttpPut("{boardId:int}")]
    public async Task<ActionResult<BoardResponseDto>> UpdateBoard(int boardId, [FromBody] BoardRequestDto boardDto)
    {
        var board = await _boardService.UpdateBoardAsync(boardId, boardDto);
        return Ok(board);
    }

    [HttpDelete("{boardId:int}")]
    public async Task<IActionResult> DeleteBoard(int boardId)
    {
        await _boardService.DeleteBoardAsync(boardId);
        return NoContent();
    }

    [HttpPost("{boardId:int}/members")]
    public async Task<ActionResult<MemberResponseDto>> AddBoardMember(int boardId, [FromBody] MemberRequestDto dto)
    {
        var result = await _boardService.AddBoardMemberAsync(boardId, dto);
        return Ok(result);
    }

    [HttpDelete("{boardId:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveBoardMember(int boardId, int userId)
    {
        var requestingUserId = _userContext.GetCurrentUserId();
        await _boardService.RemoveBoardMemberAsync(boardId, userId, requestingUserId);
        return NoContent();
    }

    [HttpGet("{boardId:int}/tasklists")]
    public async Task<ActionResult<IEnumerable<TaskListResponseDto>>> GetBoardTaskLists(
        int boardId,
        [FromQuery] TaskFilterDto filterDto)
    {
        var result = await _boardService.GetFilteredBoardTasksAsync(boardId, filterDto);
        return Ok(result);
    }

    [HttpPost("{boardId:int}/labels")]
    public async Task<ActionResult<LabelResponseDto>> CreateLabel(int boardId, [FromBody] LabelRequestDto labelDto)
    {
        var label = await _labelService.CreateLabelAsync(labelDto, boardId);

        return CreatedAtAction(
            actionName: nameof(LabelController.GetLabel),
            controllerName: "Label",
            routeValues: new { labelId = label.Id },
            value: label
        );
    }

    [HttpPost("{boardId:int}/lists")]
    public async Task<ActionResult<TaskListResponseDto>> CreateTaskList(
        int boardId,
        [FromBody] TaskListRequestDto taskListDto)
    {
        var taskList = await _taskListService.CreateTaskListAsync(taskListDto, boardId);

        return CreatedAtAction(
            actionName: nameof(TaskListController.GetTaskList),
            controllerName: "TaskList",
            routeValues: new { listId = taskList.Id },
            value: taskList
        );
    }
}