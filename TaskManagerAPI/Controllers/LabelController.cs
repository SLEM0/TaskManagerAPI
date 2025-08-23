using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabelController : ControllerBase
{
    private readonly ILabelService _labelService;

    public LabelController(ILabelService labelService)
    {
        _labelService = labelService;
    }

    [HttpGet("{labelId}")]
    public async Task<ActionResult<LabelResponseDto>> GetLabel(int labelId)
    {
        try
        {
            var label = await _labelService.GetLabelDetailsAsync(labelId);
            return Ok(label);
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

    [HttpPut("{labelId}")]
    public async Task<ActionResult<LabelResponseDto>> UpdateLabel(int labelId, [FromBody] LabelRequestDto labelDto)
    {
        try
        {
            var label = await _labelService.UpdateLabelAsync(labelId, labelDto);
            return Ok(label);
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

    [HttpDelete("{labelId}")]
    public async Task<IActionResult> DeleteLabel(int labelId)
    {
        try
        {
            await _labelService.DeleteLabelAsync(labelId);
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
}