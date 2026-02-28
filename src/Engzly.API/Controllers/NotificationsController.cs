using Engzly.Application.Interfaces;
using Engzly.Shared.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers;

public sealed class NotificationsController(INotificationService notificationService) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> SendNotification(SendNotificationCommand command)
    {
        await notificationService.SendNotificationAsync([], command.Body, command.Body);
        return Accepted();
    }
}