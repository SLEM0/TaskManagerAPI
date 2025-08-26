using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IUserContext _userContext;

    public CommentsController(ICommentService commentService, IUserContext userContext)
    {
        _commentService = commentService;
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponseDto>> AddComment(
        int taskId, [FromBody] CreateCommentRequestDto dto)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var comment = await _commentService.AddCommentAsync(taskId, dto, userId);
            return Ok(comment);
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
}