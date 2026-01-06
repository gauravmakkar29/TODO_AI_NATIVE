using TodoApi.Models;

namespace TodoApi.Services;

public interface INotificationService
{
    Task SendReminderAsync(Todo todo, User user);
    Task SendOverdueNotificationAsync(Todo todo, User user);
}

