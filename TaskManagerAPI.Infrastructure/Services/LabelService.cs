using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class LabelService : ILabelService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _authService;

    public LabelService(AppDbContext context, ICheckAccessService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<LabelResponseDto> GetLabelDetailsAsync(int labelId)
    {
        var label = await _context.Labels
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Id == labelId);

        if (label == null)
            throw new KeyNotFoundException("Label not found");

        // Проверяем доступ к доске метки
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(label.BoardId, BoardRole.Viewer);
        if (!hasAccess) throw new UnauthorizedAccessException();

        return new LabelResponseDto
        {
            Id = label.Id,
            Name = label.Name,
            Color = label.Color,
            CreatedAt = label.CreatedAt,
            BoardId = label.BoardId
        };
    }

    public async Task<LabelResponseDto> CreateLabelAsync(LabelRequestDto labelDto, int boardId)
    {
        // Проверяем доступ к доске (только редакторы и владельцы могут создавать метки)
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var label = new Label
        {
            Name = labelDto.Name,
            Color = labelDto.Color,
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Labels.Add(label);
        await _context.SaveChangesAsync();

        return new LabelResponseDto
        {
            Id = label.Id,
            Name = label.Name,
            Color = label.Color,
            CreatedAt = label.CreatedAt,
            BoardId = label.BoardId
        };
    }

    public async Task<LabelResponseDto> UpdateLabelAsync(int labelId, LabelRequestDto labelDto)
    {
        var label = await _context.Labels
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Id == labelId);

        if (label == null)
            throw new KeyNotFoundException("Label not found");

        // Проверяем доступ к доске метки (только редакторы и владельцы)
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(label.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        label.Name = labelDto.Name;
        label.Color = labelDto.Color;

        await _context.SaveChangesAsync();

        return new LabelResponseDto
        {
            Id = label.Id,
            Name = label.Name,
            Color = label.Color,
            CreatedAt = label.CreatedAt,
            BoardId = label.BoardId
        };
    }

    public async System.Threading.Tasks.Task DeleteLabelAsync(int labelId)
    {
        var label = await _context.Labels
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Id == labelId);

        if (label == null)
            throw new KeyNotFoundException("Label not found");

        // Проверяем доступ к доске метки (только редакторы и владельцы)
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(label.BoardId, BoardRole.Editor);
        if (!hasAccess) throw new UnauthorizedAccessException();

        _context.Labels.Remove(label);
        await _context.SaveChangesAsync();
    }
}