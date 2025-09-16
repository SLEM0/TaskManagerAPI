using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Application.Dtos.Label;

public class LabelRequestDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "Invalid HEX color")]
    public string Color { get; set; }
}