namespace TaskManagerAPI.Application.Dtos.Auth;
public class AuthResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}