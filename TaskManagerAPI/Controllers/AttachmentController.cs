using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/attachments")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
    private readonly IUserContext _userContext;

    public AttachmentController(IAttachmentService attachmentService, IUserContext userContext)
    {
        _attachmentService = attachmentService;
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<ActionResult<AttachmentResponseDto>> AddAttachment(int taskId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var userId = _userContext.GetCurrentUserId();
            var attachment = await _attachmentService.AddAttachmentAsync(taskId, file, userId);
            return Ok(attachment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> RemoveAttachment(int taskId, int attachmentId)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            await _attachmentService.RemoveAttachmentAsync(taskId, attachmentId, userId);
            return NoContent();
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