namespace Engzly.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(IEnumerable<string> deviceTokens, string title, string body);
}