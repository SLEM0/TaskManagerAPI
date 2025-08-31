using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Infrastructure.Options;

namespace TaskManagerAPI.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendDueDateReminderAsync(string email, string taskTitle, DateTime dueDate)
    {
        var subject = $"Напоминание: задача \"{taskTitle}\" истекает через 24 часа";
        var body = BuildEmailBody(taskTitle, dueDate);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TaskManagerAPI", _smtpSettings.FromEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpSettings.FromEmail, _smtpSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private string BuildEmailBody(string taskTitle, DateTime dueDate)
    {
        return $"""
        <!DOCTYPE html>
        <html>
          <head>
            <meta charset="utf-8">
          </head>
          <body>
            <div class="container">
              <div class="header">
                <h1>Напоминание о задаче</h1>
              </div>
              <div class="content">
                <p class="task-title">Здравствуйте! Напоминаем, что у вас есть задача {taskTitle}</p>
                <p class="due-date">Срок выполнения: {dueDate:dd.MM.yyyy в HH:mm}</p>           
                <p>Не забудьте завершить задачу вовремя!</p>
              </div>
            </div>
          </body>
        </html>
        """;
    }

    // Дополнительный метод для массовой отправки
    public async Task SendDueDateRemindersAsync(IEnumerable<string> emails, string taskTitle, DateTime dueDate)
    {
        var sendTasks = emails.Select(email => SendDueDateReminderAsync(email, taskTitle, dueDate));
        await Task.WhenAll(sendTasks);
    }
}