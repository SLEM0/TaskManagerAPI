using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Application.Interfaces.Services;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IUserContext _userContext;

    public CommentController(ICommentService commentService, IUserContext userContext)
    {
        _commentService = commentService;
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponseDto>> AddComment(int taskId, [FromBody] CommentRequestDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        var comment = await _commentService.AddCommentAsync(taskId, dto, userId);
        return Ok(comment);
    }
}