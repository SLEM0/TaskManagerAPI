using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Task;

public class MoveTaskRequestDto
{
    public int NewListId { get; set; }
    [Required]
    public int NewOrder { get; set; } 
}