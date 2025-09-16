using TaskManagerAPI.Application.Dtos.Auth;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Utils;

namespace TaskManagerAPI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(UserRegisterDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            if (existingUser.IsEmailConfirmed)
                throw new ValidationException("User with this email already exists.");

            var newConfirmationCode = Random.Shared.Next(100000, 999999);

            existingUser.EmailConfirmationCode = newConfirmationCode;
            existingUser.EmailConfirmationCodeExpires = DateTime.UtcNow.AddMinutes(20);
            await _userRepository.UpdateAsync(existingUser);

            await _emailService.SendConfirmationEmailAsync(existingUser.Email, newConfirmationCode);

            return new RegisterResponseDto
            {
                Message = "Confirmation code sent. Please check your email.",
                RequiresEmailConfirmation = true
            };
        }

        var confirmationCode = Random.Shared.Next(100000, 999999);

        PasswordHasher.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            IsEmailConfirmed = false,
            EmailConfirmationCode = confirmationCode,
            EmailConfirmationCodeExpires = DateTime.UtcNow.AddMinutes(20)
        };

        await _userRepository.AddAsync(user);

        await _emailService.SendConfirmationEmailAsync(user.Email, confirmationCode);

        return new RegisterResponseDto
        {
            Message = "Registration successful. Enter the 6-digit code sent to your email.",
            RequiresEmailConfirmation = true
        };
    }

    public async Task<AuthResponseDto> LoginAsync(UserLoginDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !PasswordHasher.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            await System.Threading.Tasks.Task.Delay(1000);
            throw new ForbiddenAccessException("Invalid email or password.");
        }

        if (!user.IsEmailConfirmed)
            throw new ValidationException("Please confirm your email address before logging in.");

        var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(user);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow
        });

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 15 * 60
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (token == null)
            throw new ForbiddenAccessException("Invalid refresh token");

        var user = await _userRepository.GetByIdAsync(token.UserId);
        if (user == null)
            throw new ForbiddenAccessException("User not found");

        await _refreshTokenRepository.DeleteAsync(token);

        var (accessToken, newRefreshToken) = _tokenService.GenerateTokenPair(user);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow
        });

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 15 * 60
        };
    }

    public async System.Threading.Tasks.Task RevokeTokensAsync(int userId)
    {
        var tokens = await _refreshTokenRepository.GetByUserIdAsync(userId);
        await _refreshTokenRepository.DeleteRangeAsync(tokens);
    }

    public async Task<ConfirmEmailResponseDto> ConfirmEmailAsync(int code)
    {
        var user = await _userRepository.GetByConfirmationCodeAsync(code);

        if (user == null)
            throw new ValidationException("Invalid or expired confirmation token.");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationCode = 0;
        user.EmailConfirmationCodeExpires = null;

        await _userRepository.UpdateAsync(user);

        return new ConfirmEmailResponseDto { Message = "Email confirmed successfully." };
    }
}