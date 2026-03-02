using Engzly.Application.Interfaces;
using Engzly.Shared.Commands;
using MassTransit;

namespace Engzly.Infrastructure.Notifications;



internal sealed class NotificationService(ISendEndpointProvider sendEndpointProvider) : INotificationService
{
    public async Task SendNotificationAsync(IEnumerable<string> deviceTokens, string title, string body)
    {
        var sendEndpoint =  await sendEndpointProvider.GetSendEndpoint(new Uri("queue:send-notification"));
        var command = new SendNotificationCommand(deviceTokens, title, body);
        await sendEndpoint.Send(command);
    }
}