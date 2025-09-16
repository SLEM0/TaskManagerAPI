using System.Text.Json;

namespace TaskManagerAPI.Application.Dtos;

public class ProblemDetailsDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public IDictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>();

    public override string ToString() => JsonSerializer.Serialize(this);
}