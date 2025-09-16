using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Interfaces.Services;

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

    [HttpGet("{labelId:int}")]
    public async Task<ActionResult<LabelResponseDto>> GetLabel(int labelId)
    {
        var label = await _labelService.GetLabelDetailsAsync(labelId);
        return Ok(label);
    }

    [HttpPut("{labelId:int}")]
    public async Task<ActionResult<LabelResponseDto>> UpdateLabel(int labelId, [FromBody] LabelRequestDto labelDto)
    {
        var label = await _labelService.UpdateLabelAsync(labelId, labelDto);
        return Ok(label);
    }

    [HttpDelete("{labelId:int}")]
    public async Task<IActionResult> DeleteLabel(int labelId)
    {
        await _labelService.DeleteLabelAsync(labelId);
        return NoContent();
    }
}