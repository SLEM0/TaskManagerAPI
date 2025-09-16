using TaskManagerAPI.Application.Dtos.Auth;

namespace TaskManagerAPI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(UserLoginDto request);
    Task<RegisterResponseDto> RegisterAsync(UserRegisterDto request);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task RevokeTokensAsync(int userId);
    Task<ConfirmEmailResponseDto> ConfirmEmailAsync(int token);
}