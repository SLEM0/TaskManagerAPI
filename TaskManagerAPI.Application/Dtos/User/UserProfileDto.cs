namespace TaskManagerAPI.Application.Dtos.User;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AvatarUrl { get; set; }
}