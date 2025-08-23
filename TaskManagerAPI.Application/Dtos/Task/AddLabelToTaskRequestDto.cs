using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Task;

public class AddLabelToTaskRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int LabelId { get; set; }
}