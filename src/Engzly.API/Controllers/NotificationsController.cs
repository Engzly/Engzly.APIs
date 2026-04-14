using Engzly.Application.Features.Notifications.Commands;
using Engzly.Application.Features.Notifications.Queries;
using Engzly.Application.Interfaces;
using Engzly.Shared.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers;

public sealed class NotificationsController(
    INotificationService notificationService,
    ISender mediator) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> SendNotification(SendNotificationCommand command)
    {
        await notificationService.SendNotificationAsync([], command.Body, command.Body);
        return Accepted();
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] bool unreadOnly = false)
    {
        var result = await mediator.Send(new GetMyNotificationsQuery(unreadOnly));
        return Resolve(result);
    }

    [Authorize]
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await mediator.Send(new GetUnreadNotificationsCountQuery());
        return Resolve(result);
    }

    [Authorize]
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] Guid id)
    {
        var result = await mediator.Send(new MarkNotificationReadCommand(id));
        return Resolve(result);
    }
}