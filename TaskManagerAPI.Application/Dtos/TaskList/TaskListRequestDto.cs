using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.TaskList;

public class TaskListRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; }
}