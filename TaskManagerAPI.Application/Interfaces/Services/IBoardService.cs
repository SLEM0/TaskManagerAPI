using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Application.Dtos.TaskList;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface IBoardService
{
    Task<IEnumerable<ShortBoardResponseDto>> GetUserBoardsAsync(int userId);
    Task<BoardResponseDto> GetBoardDetailsAsync(int boardId);
    Task<BoardResponseDto> CreateBoardAsync(BoardRequestDto dto, int usedId);
    Task<BoardResponseDto> UpdateBoardAsync(int boardId, BoardRequestDto dto);
    Task DeleteBoardAsync(int boardId);
    Task<BoardResponseDto> AddBoardMemberAsync(int boardId, MemberRequestDto dto);
    Task RemoveBoardMemberAsync(int boardId, int userId, int requestingUserId);
    Task<IEnumerable<TaskListResponseDto>> GetFilteredBoardTasksAsync(int boardId, TaskFilterDto filterDto);
}