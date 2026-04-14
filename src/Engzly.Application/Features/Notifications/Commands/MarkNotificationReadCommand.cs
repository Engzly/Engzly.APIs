using Engzly.Application.Common.Bases;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Notifications;
using MediatR;

namespace Engzly.Application.Features.Notifications.Commands
{
    public sealed record MarkNotificationReadCommand(Guid NotificationId)
        : IRequest<Response<string>>;

    public sealed class MarkNotificationReadCommandHandler(
        IGenericRepository<Notification, Guid> _repo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<MarkNotificationReadCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            MarkNotificationReadCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<string>();

            var notification = await _repo.GetByIdAsync(request.NotificationId, cancellationToken);
            if (notification is null)
                return NotFound<string>("Notification not found");

            if (notification.UserId != caller.Id)
                return Forbidden<string>("You can only mark your own notifications");

            if (notification.IsRead)
                return Success(notification.Id.ToString(), "Already read");

            notification.IsRead = true;
            _repo.Update(notification);
            await _repo.CompleteAsync(cancellationToken);

            return Success(notification.Id.ToString(), "Marked as read");
        }
    }
}
