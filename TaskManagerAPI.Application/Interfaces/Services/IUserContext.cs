namespace TaskManagerAPI.Application.Interfaces.Services;

public interface IUserContext
{
    int GetCurrentUserId();
    string GetCurrentUserName();
}