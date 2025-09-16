namespace TaskManagerAPI.Application.Dtos.Label;

public class LabelResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BoardId { get; set; }
}