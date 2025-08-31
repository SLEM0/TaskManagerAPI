namespace TaskManagerAPI.Application.Interfaces;

public interface IEmailService
{
    Task SendDueDateReminderAsync(string email, string taskTitle, DateTime dueDate);
}