using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Domain.Entities;

public class Label
{
    public int Id { get; set; }
    public string Name { get; set; }

    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "Invalid HEX color")]
    public string Color { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int BoardId { get; set; }
    public Board Board { get; set; }
}