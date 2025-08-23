using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Task;

public class RemoveLabelRequestDto
{
    [Required]
    public int LabelId { get; set; }
}