using AutoMapper;
using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Infrastructure.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly ICheckAccessService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public BoardService(
        IBoardRepository boardRepository,
        ICheckAccessService authService,
        IUserRepository userRepository,
        IMemberRepository memberRepository,
        IMapper mapper)
    {
        _boardRepository = boardRepository;
        _authService = authService;
        _userRepository = userRepository;
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<BoardResponseDto> CreateBoardAsync(BoardRequestDto dto, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User not found");

        var board = new Board
        {
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _boardRepository.AddAsync(board);

        var boardMember = new Member
        {
            BoardId = board.Id,
            UserId = userId,
            Role = BoardRole.Owner,
            AddedAt = DateTime.UtcNow
        };

        await _memberRepository.AddAsync(boardMember);

        var boardWithDetails = await _boardRepository.GetByIdWithDetailsAsync(board.Id);
        if (boardWithDetails == null) throw new NotFoundException("Board not found after creation");

        return _mapper.Map<BoardResponseDto>(boardWithDetails, opts =>
            opts.Items["IsOwner"] = true);
    }

    public async Task<IEnumerable<ShortBoardResponseDto>> GetUserBoardsAsync(int userId)
    {
        var boards = await _boardRepository.GetUserBoardsAsync(userId);

        return boards.Select(board =>
        {
            var dto = _mapper.Map<ShortBoardResponseDto>(board);
            dto.IsOwner = board.OwnerId == userId;
            return dto;
        });
    }

    public async Task<BoardResponseDto> GetBoardDetailsAsync(int boardId)
    {
        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        if (!hasAccess) throw new ForbiddenAccessException();

        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new NotFoundException("Board not found");

        return _mapper.Map<BoardResponseDto>(board, opts =>
            opts.Items["IsOwner"] = isOwner);
    }

    public async Task<BoardResponseDto> UpdateBoardAsync(int boardId, BoardRequestDto dto)
    {
        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new ForbiddenAccessException();

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null) throw new NotFoundException("Board not found");

        board.Title = dto.Title;
        board.Description = dto.Description;

        await _boardRepository.UpdateAsync(board);

        var updatedBoard = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (updatedBoard == null) throw new NotFoundException("Board not found after update");

        return _mapper.Map<BoardResponseDto>(updatedBoard, opts =>
            opts.Items["IsOwner"] = isOwner);
    }

    public async System.Threading.Tasks.Task DeleteBoardAsync(int boardId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new ForbiddenAccessException();

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null) throw new NotFoundException("Board not found");

        await _boardRepository.DeleteAsync(board);
    }

    public async Task<BoardResponseDto> AddBoardMemberAsync(int boardId, MemberRequestDto dto)
    {
        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new ForbiddenAccessException();

        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLower());
        if (user == null) throw new NotFoundException("User not found");

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null) throw new NotFoundException("Board not found");
        if (board.OwnerId == user.Id) throw new ValidationException("User is the board owner");

        var alreadyMember = await _memberRepository.ExistsAsync(boardId, user.Id);
        if (alreadyMember) throw new ValidationException("User is already a member");

        var boardMember = new Member
        {
            BoardId = boardId,
            UserId = user.Id,
            Role = dto.Role,
            AddedAt = DateTime.UtcNow
        };

        await _memberRepository.AddAsync(boardMember);

        var updatedBoard = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (updatedBoard == null) throw new NotFoundException("Board not found after adding member");

        return _mapper.Map<BoardResponseDto>(updatedBoard, opts =>
            opts.Items["IsOwner"] = isOwner);
    }

    public async System.Threading.Tasks.Task RemoveBoardMemberAsync(int boardId, int userId, int requestingUserId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new ForbiddenAccessException();

        if (userId == requestingUserId)
            throw new ValidationException("Cannot remove yourself");

        var boardMember = await _memberRepository.GetByBoardAndUserIdAsync(boardId, userId);
        if (boardMember == null) throw new NotFoundException("Member not found");

        await _memberRepository.DeleteAsync(boardMember);
    }

    public async Task<IEnumerable<TaskListResponseDto>> GetFilteredBoardTasksAsync(int boardId, TaskFilterDto filterDto)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        if (!hasAccess) throw new ForbiddenAccessException();

        var board = await _boardRepository.GetBoardWithTasksAsync(boardId);
        if (board == null) throw new NotFoundException("Board not found");

        var allTasks = board.Lists
            .SelectMany(l => l.Tasks.OrderBy(t => t.Order))
            .ToList();

        var filteredTasks = ApplyFilters(allTasks.AsQueryable(), filterDto).ToList();

        var result = board.Lists
            .OrderBy(l => l.Order)
            .Where(list => filteredTasks.Any(t => t.TaskListId == list.Id))
            .Select(list =>
            {
                var listDto = _mapper.Map<TaskListResponseDto>(list);
                listDto.Tasks = filteredTasks
                    .Where(t => t.TaskListId == list.Id)
                    .Select(t => _mapper.Map<ShortTaskResponseDto>(t))
                    .ToList();
                return listDto;
            })
            .ToList();

        return result;
    }

    private IQueryable<Domain.Entities.Task> ApplyFilters(IQueryable<Domain.Entities.Task> query, TaskFilterDto filterDto)
    {
        if (filterDto.LabelIds != null && filterDto.LabelIds.Any())
        {
            query = query.Where(t => t.Labels.Any(l => filterDto.LabelIds.Contains(l.Id)));
        }

        if (filterDto.MemberIds != null && filterDto.MemberIds.Any())
        {
            query = query.Where(t => t.Members.Any(a => filterDto.MemberIds.Contains(a.UserId)));
        }

        if (filterDto.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == filterDto.IsCompleted.Value);
        }

        if (filterDto.DueDatePreset.HasValue)
        {
            var now = DateTime.UtcNow;

            query = filterDto.DueDatePreset.Value switch
            {
                DueDateFilter.NoDate => query.Where(t => t.DueDate == null),
                DueDateFilter.Expired => query.Where(t => t.DueDate != null && t.DueDate < now),
                DueDateFilter.DueWithinDay => query.Where(t => t.DueDate != null &&
                    t.DueDate >= now && t.DueDate <= now.AddHours(24)),
                DueDateFilter.DueWithinWeek => query.Where(t => t.DueDate != null &&
                    t.DueDate >= now && t.DueDate <= now.AddDays(7)),
                DueDateFilter.DueWithinMonth => query.Where(t => t.DueDate != null &&
                    t.DueDate >= now && t.DueDate <= now.AddDays(30)),
                _ => query
            };
        }

        return query;
    }
}