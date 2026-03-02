using Engzly.Shared.Commands;
using FirebaseAdmin.Messaging;
namespace Engzly.Notification.Services;

public class FirebaseNotificationService : INotificationService
{
    public async Task<string> SendToDeviceAsync(SendNotificationCommand command)
    {
        var message = new Message
        {
            Token = command.DeviceTokens.Single(),
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = command.Title,
                Body = command.Body
            }
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task SendToMultipleAsync(SendNotificationCommand command)
    {
        var deviceTokens = command.DeviceTokens.ToList();
        var message = new MulticastMessage
        {
            Tokens = deviceTokens,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = command.Title,
                Body = command.Body
            }
        };

        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

        for (int i = 0; i < response.Responses.Count; i++)
        {
            if (!response.Responses[i].IsSuccess)
            {
                var failedToken = deviceTokens[i];

                // TODO: Deactivate inactive tokens.
                Console.WriteLine($"Failed token: {failedToken}");
            }
        }
    }
}