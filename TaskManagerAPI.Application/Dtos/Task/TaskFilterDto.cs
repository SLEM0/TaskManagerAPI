using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Application.Dtos.Task;

public class TaskFilterDto
{
    public DueDateFilter? DueDatePreset { get; set; }
    public List<int> LabelIds { get; set; } = new List<int>();
    public List<int> MemberIds { get; set; } = new List<int>();
    public bool? IsCompleted { get; set; }
}