using TaskManagerAPI.Application.Dtos.Task;

namespace TaskManagerAPI.Application.Dtos.TaskList;

public class TaskListResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int BoardId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Order { get; set; }
    public IEnumerable<ShortTaskResponseDto> Tasks { get; set; }
}