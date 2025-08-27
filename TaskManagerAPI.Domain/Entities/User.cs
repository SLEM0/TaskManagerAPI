namespace TaskManagerAPI.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string AvatarUrl { get; set; } = "/avatars/default-avatar.png";
}
