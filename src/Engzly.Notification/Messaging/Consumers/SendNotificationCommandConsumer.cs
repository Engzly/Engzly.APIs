using Engzly.Notification.Services;
using Engzly.Shared.Commands;
using MassTransit;

namespace Engzly.Notification.Messaging.Consumers;

internal sealed class SendNotificationCommandConsumer(INotificationService notificationService) : IConsumer<SendNotificationCommand>
{
    public Task Consume(ConsumeContext<SendNotificationCommand> context)
    {
        Task.Delay(10);
        // await notificationService.SendToMultipleAsync(context.Message);
        Console.WriteLine($"I Have Received this notification {context.Message.Title} : {context.Message.Body}");
        return Task.CompletedTask;
    }
}