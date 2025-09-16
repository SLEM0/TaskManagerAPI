using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.TaskList;

namespace TaskManagerAPI.Application.Dtos.Board;

public class BoardResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOwner { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } 
    public IEnumerable<TaskListResponseDto> Lists { get; set; }
    public IEnumerable<LabelResponseDto> Labels { get; set; }
    public IEnumerable<MemberResponseDto> Members { get; set; }
}