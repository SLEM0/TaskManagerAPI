namespace TaskManagerAPI.Application.Dtos.Attachment;

public class AttachmentResponseDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int UploadedById { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}