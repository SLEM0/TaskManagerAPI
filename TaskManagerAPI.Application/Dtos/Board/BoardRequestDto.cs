using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Board;

public class BoardRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}