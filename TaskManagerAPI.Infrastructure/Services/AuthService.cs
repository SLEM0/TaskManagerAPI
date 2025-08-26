using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Dtos.Auth;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Data;
using TaskManagerAPI.Infrastructure.Utils;

namespace TaskManagerAPI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(UserRegisterDto request)
    {
        // Проверка существования пользователя
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new ArgumentException("User with this email already exists.");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new ArgumentException("User with this username already exists.");

        // Создание хеша пароля
        PasswordHasher.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        // Создание пользователя
        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterResponseDto { Message = "User registered successfully." };
    }

    public async Task<AuthResponseDto> LoginAsync(UserLoginDto request)
    {
        var tableExists = await _context.Database.CanConnectAsync();
        if (!tableExists)
        {
            throw new InvalidOperationException("Database not ready");
        }
        // Поиск пользователя
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !PasswordHasher.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            await System.Threading.Tasks.Task.Delay(1000); // Задержка при неверных credentials
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Генерация токенов
        var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(user);

        // Сохранение refresh token
        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 15 * 60
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Поиск валидного refresh token
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsExpired);

        if (token == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Удаление использованного токена
        _context.RefreshTokens.Remove(token);

        // Генерация новой пары токенов
        var (accessToken, newRefreshToken) = _tokenService.GenerateTokenPair(token.User);

        // Сохранение нового refresh token
        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = token.User.Id,
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 15 * 60
        };
    }

    public async System.Threading.Tasks.Task RevokeTokensAsync(int userId)
    {
        // Удаление всех refresh tokens пользователя
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
}