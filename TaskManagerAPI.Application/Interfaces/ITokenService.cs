using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Interfaces;

public interface ITokenService
{
    (string accessToken, string refreshToken) GenerateTokenPair(User user);
}