using TaskManagerAPI.Domain.Enums;

namespace TaskManagerAPI.Application.Dtos.Task;

public class TaskFilterDto
{
    public DueDateFilter? DueDatePreset { get; set; } // Используем enum для предустановок
    public List<int> LabelIds { get; set; } = new List<int>();
    public bool? IsCompleted { get; set; }
}