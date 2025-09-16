using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Task;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface ILabelService
{
    Task<LabelResponseDto> GetLabelDetailsAsync(int labelId);
    Task<LabelResponseDto> CreateLabelAsync(LabelRequestDto labelDto, int boardId);
    Task<LabelResponseDto> UpdateLabelAsync(int labelId, LabelRequestDto labelDto);
    Task DeleteLabelAsync(int labelId);
}