using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15); // Check every 15 minutes

    public ReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminders");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var reminderWindowStart = now;
        var reminderWindowEnd = now.AddMinutes(15); // 15-minute window for reminders

        // Find todos with reminders that should be sent
        var todosToRemind = await context.Todos
            .Include(t => t.User)
            .Where(t => 
                !t.IsCompleted &&
                t.ReminderDate.HasValue &&
                t.ReminderDate.Value >= reminderWindowStart &&
                t.ReminderDate.Value <= reminderWindowEnd &&
                !t.DueDate.HasValue || t.DueDate.Value >= now) // Not overdue yet
            .ToListAsync();

        foreach (var todo in todosToRemind)
        {
            if (todo.User != null)
            {
                await notificationService.SendReminderAsync(todo, todo.User);
                // Mark reminder as sent by clearing it (or you could add a ReminderSent field)
                todo.ReminderDate = null;
            }
        }

        // Find overdue todos that haven't been notified recently
        var overdueTodos = await context.Todos
            .Include(t => t.User)
            .Where(t => 
                !t.IsCompleted &&
                t.DueDate.HasValue &&
                t.DueDate.Value < now)
            .ToListAsync();

        foreach (var todo in overdueTodos)
        {
            if (todo.User != null)
            {
                // Send overdue notification (you might want to add a field to track if already notified)
                await notificationService.SendOverdueNotificationAsync(todo, todo.User);
            }
        }

        if (todosToRemind.Any() || overdueTodos.Any())
        {
            await context.SaveChangesAsync();
        }

        if (todosToRemind.Any())
        {
            _logger.LogInformation("Processed {Count} reminders", todosToRemind.Count);
        }
    }
}

