using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Application.Interfaces.Services;

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
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        var userId = _userContext.GetCurrentUserId();
        var attachment = await _attachmentService.AddAttachmentAsync(taskId, file, userId);
        return Ok(attachment);
    }

    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> RemoveAttachment(int taskId, int attachmentId)
    {
        var userId = _userContext.GetCurrentUserId();
        await _attachmentService.RemoveAttachmentAsync(taskId, attachmentId, userId);
        return NoContent();
    }
}