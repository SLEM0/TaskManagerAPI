using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagerAPI.Application.Interfaces;

namespace TaskManagerAPI.Application.BackgroundServices;

public class DueDateNotificationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10); // Проверяем каждый час

    public DueDateNotificationWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await CheckAndSendNotificationsAsync(taskService, emailService);
            }
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndSendNotificationsAsync(ITaskService taskService, IEmailService emailService)
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddHours(24); // Задачи, которые истекают через 24 часа

        // Ищем задачи с DueDate между сейчас и через 24 часа
        var tasksDueSoon = await taskService.GetTasksDueBetweenAsync(now, threshold);

        foreach (var task in tasksDueSoon)
        {
            // Проверяем, не отправляли ли уже уведомление
            if (!task.DueDateNotificationSent)
            {
                foreach (var member in task.Members)
                {
                    await emailService.SendDueDateReminderAsync(
                        member.User.Email,
                        task.Title,
                        task.DueDate.Value
                    );
                }

                // Помечаем, что уведомление отправлено
                await taskService.MarkDueDateNotificationSentAsync(task.Id);
            }
        }
    }
}