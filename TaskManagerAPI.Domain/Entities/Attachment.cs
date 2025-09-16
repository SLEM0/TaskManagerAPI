namespace TaskManagerAPI.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;

    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}