using Engzly.Shared.Commands;

namespace Engzly.Notification.Services;

public interface INotificationService
{
    Task<string> SendToDeviceAsync(SendNotificationCommand command);
    Task SendToMultipleAsync(SendNotificationCommand command);
}