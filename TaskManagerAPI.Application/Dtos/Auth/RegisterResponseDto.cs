namespace TaskManagerAPI.Application.Dtos.Auth;

public class RegisterResponseDto
{
    public string Message { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
}