using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Task;

public class AddLabelRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int LabelId { get; set; }
}