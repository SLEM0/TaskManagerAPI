using AutoMapper;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepository;
    private readonly ICheckAccessService _authService;
    private readonly IMapper _mapper;

    public LabelService(
        ILabelRepository labelRepository,
        ICheckAccessService authService,
        IMapper mapper)
    {
        _labelRepository = labelRepository;
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<LabelResponseDto> GetLabelDetailsAsync(int labelId)
    {
        var label = await _labelRepository.GetByIdWithBoardAsync(labelId);
        if (label == null) throw new NotFoundException("Label not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(label.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new ForbiddenAccessException();

        return _mapper.Map<LabelResponseDto>(label);
    }

    public async Task<LabelResponseDto> CreateLabelAsync(LabelRequestDto labelDto, int boardId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        var label = new Label
        {
            Name = labelDto.Name,
            Color = labelDto.Color,
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow
        };

        await _labelRepository.AddAsync(label);

        var createdLabel = await _labelRepository.GetByIdWithBoardAsync(label.Id);
        if (createdLabel == null) throw new NotFoundException("Label not found after creation");

        return _mapper.Map<LabelResponseDto>(createdLabel);
    }

    public async Task<LabelResponseDto> UpdateLabelAsync(int labelId, LabelRequestDto labelDto)
    {
        var label = await _labelRepository.GetByIdWithBoardAsync(labelId);
        if (label == null) throw new NotFoundException("Label not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(label.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        label.Name = labelDto.Name;
        label.Color = labelDto.Color;

        await _labelRepository.UpdateAsync(label);

        var updatedLabel = await _labelRepository.GetByIdWithBoardAsync(labelId);
        if (updatedLabel == null) throw new NotFoundException("Label not found after update");

        return _mapper.Map<LabelResponseDto>(updatedLabel);
    }

    public async System.Threading.Tasks.Task DeleteLabelAsync(int labelId)
    {
        var label = await _labelRepository.GetByIdWithBoardAsync(labelId);
        if (label == null) throw new NotFoundException("Label not found");

        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(label.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new ForbiddenAccessException();

        await _labelRepository.DeleteAsync(label);
    }
}