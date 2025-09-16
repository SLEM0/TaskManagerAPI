namespace TaskManagerAPI.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendDueDateReminderAsync(string email, string taskTitle, DateTime dueDate);
    Task SendConfirmationEmailAsync(string email, int confirmationCode);
}