using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Infrastructure.Options;

namespace TaskManagerAPI.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    private async Task SendEmailAsync(string email, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TaskManagerAPI", _smtpSettings.FromEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpSettings.FromEmail, _smtpSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendDueDateReminderAsync(string email, string taskTitle, DateTime dueDate)
    {
        var subject = $"Напоминание: задача \"{taskTitle}\" истекает через 24 часа";
        var body = BuildDueDateReminderBody(taskTitle, dueDate);

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendConfirmationEmailAsync(string email, int confirmationCode)
    {
        var subject = "Confirm your email address";
        var body = BuildConfirmationEmailBody(confirmationCode);

        await SendEmailAsync(email, subject, body);
    }

    private string BuildDueDateReminderBody(string taskTitle, DateTime dueDate)
    {
        return $"""
        <!DOCTYPE html>
        <html>
          <head><meta charset="utf-8"></head>
          <body>
              <h2>Task reminder</h2>
              <p>Hello! We remind you that you have a task: {taskTitle}</p>
              <p>Due date: {dueDate:dd.MM.yyyy в HH:mm}</p>
              <p>Don't forget to complete the task on time!</p>
          </body>
        </html>
        """;
    }

    private string BuildConfirmationEmailBody(int confirmationCode)
    {
        return $"""
        <!DOCTYPE html>
        <html>
          <body>
              <h2>Welcome to TaskManagerAPI!</h2>
              <p>Please confirm your email address using the following code:</p>
                <h1>
                  {confirmationCode}
                </h1>
              <p>Enter this code on the confirmation page to complete your registration. This code will expire in 20 minutes. If you didn't request this code, please ignore this email.</p>
          </body>
        </html>
        </html>
        """;
    }

    public async Task SendDueDateRemindersAsync(IEnumerable<string> emails, string taskTitle, DateTime dueDate)
    {
        var sendTasks = emails.Select(email => SendDueDateReminderAsync(email, taskTitle, dueDate));
        await Task.WhenAll(sendTasks);
    }
}