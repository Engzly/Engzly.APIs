using Engzly.Application.Common.Bases;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Notifications;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Notifications.Queries
{
    public sealed class GetMyNotificationsQueryHandler(
        IGenericRepository<Notification, Guid> _repo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetMyNotificationsQuery, Response<IReadOnlyList<NotificationItemDto>>>
    {
        public async Task<Response<IReadOnlyList<NotificationItemDto>>> Handle(
            GetMyNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<IReadOnlyList<NotificationItemDto>>();

            var spec = request.UnreadOnly
                ? new NotificationsForUserSpec(caller.Id, onlyUnread: true)
                : new NotificationsForUserSpec(caller.Id, onlyUnread: false);

            var notifications = await _repo.GetAllAsync(spec, cancellationToken);
            var items = notifications
                .OrderByDescending(n => n.CreatedOn)
                .Select(n => new NotificationItemDto(n.Id, n.Title, n.Body, n.CreatedOn, n.IsRead))
                .ToList();

            return Success<IReadOnlyList<NotificationItemDto>>(items);
        }
    }

    public sealed class GetUnreadNotificationsCountQueryHandler(
        IGenericRepository<Notification, Guid> _repo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetUnreadNotificationsCountQuery, Response<int>>
    {
        public async Task<Response<int>> Handle(
            GetUnreadNotificationsCountQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<int>();

            var count = await _repo.CountAsync(
                new NotificationsForUserSpec(caller.Id, onlyUnread: true),
                cancellationToken);
            return Success(count);
        }
    }

    internal sealed class NotificationsForUserSpec : BaseSpecification<Notification>
    {
        public NotificationsForUserSpec(string userId, bool onlyUnread)
            : base(n => n.UserId == userId && (!onlyUnread || !n.IsRead))
        { }
    }
}
