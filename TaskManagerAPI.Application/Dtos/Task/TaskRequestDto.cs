using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Task;

public class TaskRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public DateTime? DueDate { get; set; }
    public bool? IsCompleted { get; set; }
}