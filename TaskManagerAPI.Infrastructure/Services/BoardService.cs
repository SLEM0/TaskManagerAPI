using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Services;

public class BoardService : IBoardService
{
    private readonly AppDbContext _context;
    private readonly ICheckAccessService _authService;

    public BoardService(AppDbContext context, ICheckAccessService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<BoardResponseDto> CreateBoardAsync(BoardRequestDto dto, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var board = new Board
        {
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardUser = new Member
        {
            BoardId = board.Id,
            UserId = userId,
            Role = BoardRole.Owner,
            AddedAt = DateTime.UtcNow
        };
        _context.Members.Add(boardUser);
        await _context.SaveChangesAsync();

        return new BoardResponseDto
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            CreatedAt = board.CreatedAt,
            IsOwner = true,
            OwnerId = userId,
            OwnerName = user.Username,
            Lists = new List<TaskListResponseDto>(),
            Labels = new List<LabelResponseDto>(),
            Members = new List<MemberResponseDto>
            {
                new MemberResponseDto
                {
                    Id = boardUser.Id,
                    BoardId = board.Id,
                    UserId = userId,
                    UserName = user.Username,
                    UserEmail = user.Email,
                    Role = BoardRole.Owner,
                    AddedAt = boardUser.AddedAt
                }
            }
        };
    }

    public async Task<IEnumerable<ShortBoardResponseDto>> GetUserBoardsAsync(int userId)
    {
        return await _context.Boards
            .Where(b => b.OwnerId == userId || b.BoardUsers.Any(bu => bu.UserId == userId))
            .Include(b => b.Owner)
            .Select(b => new ShortBoardResponseDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                CreatedAt = b.CreatedAt,
                IsOwner = b.OwnerId == userId,
                OwnerId = b.OwnerId,
                OwnerName = b.Owner.Username
            })
            .ToListAsync();
    }

    public async Task<BoardResponseDto> GetBoardDetailsAsync(int boardId)
    {
        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var board = await _context.Boards
            .Include(b => b.Owner)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Labels)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
            .Include(b => b.Labels)
            .Include(b => b.BoardUsers)
                .ThenInclude(bu => bu.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board == null) throw new KeyNotFoundException("Board not found");

        return new BoardResponseDto
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            CreatedAt = board.CreatedAt,
            IsOwner = isOwner,
            OwnerId = board.OwnerId,
            OwnerName = board.Owner.Username,
            Lists = board.Lists
                .OrderBy(l => l.Order)
                .Select(l => new TaskListResponseDto
            {
                Id = l.Id,
                Title = l.Title,
                BoardId = l.BoardId,
                CreatedAt = l.CreatedAt,
                Order = l.Order,
                Tasks = l.Tasks
                    .OrderBy(t => t.Order)
                    .Select(t => new ShortTaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    TaskListId = t.TaskListId,
                    Order = t.Order,
                    Labels = t.Labels.Select(label => new LabelResponseDto
                    {
                        Id = label.Id,
                        Name = label.Name,
                        Color = label.Color,
                        CreatedAt = label.CreatedAt,
                        BoardId = label.BoardId
                    }),
                    Members = t.Members.Select(member => new MemberResponseDto
                    {
                        Id = member.Id,
                        BoardId = member.BoardId,
                        UserId = member.User.Id,
                        UserName = member.User.Username,
                        UserEmail = member.User.Email,
                        Role = member.Role,
                        AddedAt = member.AddedAt
                    })
                    })
            }),
            Labels = board.Labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                BoardId = l.BoardId,
                CreatedAt = l.CreatedAt
            }),
            Members = board.BoardUsers.Select(bu => new MemberResponseDto
            {
                Id = bu.Id,
                BoardId = bu.BoardId,
                UserId = bu.UserId,
                UserName = bu.User.Username,
                UserEmail = bu.User.Email,
                Role = bu.Role,
                AddedAt = bu.AddedAt
            })
        };
    }

    public async Task<BoardResponseDto> UpdateBoardAsync(int boardId, BoardRequestDto dto)
    {
        var (hasAccess, isOwner) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var board = await _context.Boards
            .Include(b => b.Owner)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Labels)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
            .Include(b => b.Labels)
            .Include(b => b.BoardUsers)
                .ThenInclude(bu => bu.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board == null) throw new KeyNotFoundException("Board not found");

        board.Title = dto.Title;
        board.Description = dto.Description;

        await _context.SaveChangesAsync();

        return new BoardResponseDto
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            CreatedAt = board.CreatedAt,
            IsOwner = isOwner,
            OwnerId = board.OwnerId,
            OwnerName = board.Owner.Username,
            Lists = board.Lists
                .OrderBy(l => l.Order)
                .Select(l => new TaskListResponseDto
            {
                Id = l.Id,
                Title = l.Title,
                BoardId = l.BoardId,
                CreatedAt = l.CreatedAt,
                Order = l.Order,
                Tasks = l.Tasks
                    .OrderBy(t => t.Order)
                    .Select(t => new ShortTaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    TaskListId = t.TaskListId,
                    Order = t.Order,
                    Labels = t.Labels.Select(label => new LabelResponseDto
                    {
                        Id = label.Id,
                        Name = label.Name,
                        Color = label.Color,
                        CreatedAt = label.CreatedAt,
                        BoardId = label.BoardId
                    }),
                    Members = t.Members.Select(member => new MemberResponseDto
                    {
                        Id = member.Id,
                        BoardId = member.BoardId,
                        UserId = member.User.Id,
                        UserName = member.User.Username,
                        UserEmail = member.User.Email,
                        Role = member.Role,
                        AddedAt = member.AddedAt
                    })
                    })
            }),
            Labels = board.Labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                BoardId = l.BoardId,
                CreatedAt = l.CreatedAt
            }),
            Members = board.BoardUsers.Select(bu => new MemberResponseDto
            {
                Id = bu.Id,
                BoardId = bu.BoardId,
                UserId = bu.UserId,
                UserName = bu.User.Username,
                UserEmail = bu.User.Email,
                Role = bu.Role,
                AddedAt = bu.AddedAt
            })
        };
    }

    public async System.Threading.Tasks.Task DeleteBoardAsync(int boardId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var board = await _context.Boards.FindAsync(boardId);
        if (board == null) throw new KeyNotFoundException("Board not found");

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();
    }

    public async Task<MemberResponseDto> AddBoardMemberAsync(int boardId, MemberRequestDto dto)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (user == null) throw new KeyNotFoundException("User not found");

        var alreadyMember = await _context.Members
            .AnyAsync(bu => bu.BoardId == boardId && bu.UserId == user.Id);
        if (alreadyMember) throw new InvalidOperationException("User is already a member");

        var board = await _context.Boards.FindAsync(boardId);
        if (board == null) throw new KeyNotFoundException("Board not found");
        if (board.OwnerId == user.Id) throw new InvalidOperationException("User is the board owner");

        var boardUser = new Member
        {
            BoardId = boardId,
            UserId = user.Id,
            Role = dto.Role,
            AddedAt = DateTime.UtcNow
        };

        _context.Members.Add(boardUser);
        await _context.SaveChangesAsync();

        return new MemberResponseDto
        {
            Id = boardUser.Id,
            BoardId = boardUser.BoardId,
            UserId = boardUser.UserId,
            UserName = user.Username,
            UserEmail = user.Email,
            Role = boardUser.Role,
            AddedAt = boardUser.AddedAt
        };
    }

    public async System.Threading.Tasks.Task RemoveBoardMemberAsync(int boardId, int userId, int requestingUserId)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Owner);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var boardUser = await _context.Members
            .FirstOrDefaultAsync(bu => bu.BoardId == boardId && bu.UserId == userId);

        if (boardUser == null) throw new KeyNotFoundException("Member not found");
        if (boardUser.UserId == requestingUserId) throw new InvalidOperationException("Cannot remove yourself");

        _context.Members.Remove(boardUser);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskListResponseDto>> GetFilteredBoardTasksAsync(int boardId, TaskFilterDto filterDto)
    {
        var (hasAccess, _) = await _authService.CheckBoardAccessAsync(boardId, BoardRole.Viewer);
        if (!hasAccess) throw new UnauthorizedAccessException();

        var board = await _context.Boards
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Labels)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Tasks)
                    .ThenInclude(t => t.Members) // ← Исправляем на Assignees
                        .ThenInclude(a => a.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board == null) throw new KeyNotFoundException("Board not found");

        var allTasks = board.Lists
            .SelectMany(l => l.Tasks
                .OrderBy(t => t.Order)
            )
            .ToList();

        var filteredTasks = ApplyFilters(allTasks.AsQueryable(), filterDto).ToList();

        return board.Lists
            .OrderBy(l => l.Order)
            .Select(list => new TaskListResponseDto
            {
                Id = list.Id,
                Title = list.Title,
                BoardId = list.BoardId,
                CreatedAt = list.CreatedAt,
                Order = list.Order,
                Tasks = filteredTasks
                    .Where(t => t.TaskListId == list.Id)
                    .Select(t => new ShortTaskResponseDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        DueDate = t.DueDate,
                        IsCompleted = t.IsCompleted,
                        CreatedAt = t.CreatedAt,
                        TaskListId = t.TaskListId,
                        Order = t.Order,
                        Labels = t.Labels.Select(label => new LabelResponseDto
                        {
                            Id = label.Id,
                            Name = label.Name,
                            Color = label.Color,
                            CreatedAt = label.CreatedAt,
                            BoardId = label.BoardId
                        }),
                        Members = t.Members.Select(member => new MemberResponseDto // ← Исправляем на Assignees
                        {
                            Id = member.Id,
                            BoardId = member.BoardId,
                            UserId = member.User.Id,
                            UserName = member.User.Username,
                            UserEmail = member.User.Email,
                            Role = member.Role,
                            AddedAt = member.AddedAt
                        })
                    })
                    .ToList()
            })
            .Where(list => list.Tasks.Any())
            .ToList();
    }

    private IQueryable<Domain.Entities.Task> ApplyFilters(IQueryable<Domain.Entities.Task> query, TaskFilterDto filterDto)
    {
        // Фильтр по меткам
        if (filterDto.LabelIds != null && filterDto.LabelIds.Any())
        {
            query = query.Where(t => t.Labels.Any(l => filterDto.LabelIds.Contains(l.Id)));
        }

        // Фильтр по участникам (НОВОЕ!)
        if (filterDto.MemberIds != null && filterDto.MemberIds.Any())
        {
            query = query.Where(t => t.Members.Any(a => filterDto.MemberIds.Contains(a.UserId)));
        }

        // Фильтр по статусу выполнения
        if (filterDto.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == filterDto.IsCompleted.Value);
        }

        // Фильтр по сроку выполнения (с учетом времени!)
        if (filterDto.DueDatePreset.HasValue)
        {
            var now = DateTime.UtcNow;

            query = filterDto.DueDatePreset.Value switch
            {
                // Без срока
                DueDateFilter.NoDate => query.Where(t => t.DueDate == null),

                // Просроченные: срок меньше текущего момента
                DueDateFilter.Expired => query.Where(t => t.DueDate != null && t.DueDate < now),

                // Истекает в течение суток: срок между сейчас и через 24 часа
                DueDateFilter.DueWithinDay => query.Where(t => t.DueDate != null &&
                    t.DueDate >= now &&
                    t.DueDate <= now.AddHours(24)),

                // Истекает в течение недели: срок между сейчас и через 7 дней
                DueDateFilter.DueWithinWeek => query.Where(t => t.DueDate != null &&
                    t.DueDate >= now &&
                    t.DueDate <= now.AddDays(7)),

                // Истекает в течение месяца: срок между сейчас и через 30 дней
                DueDateFilter.DueWithinMonth => query.Where(t => t.DueDate != null &&
                    t.DueDate >= now &&
                    t.DueDate <= now.AddDays(30)),

                _ => query
            };
        }

        return query;
    }
}