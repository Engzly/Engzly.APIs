using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Notifications.Queries
{
    public sealed record NotificationItemDto(
        Guid Id,
        string Title,
        string Body,
        DateTime CreatedOn,
        bool IsRead);

    public sealed record GetMyNotificationsQuery(bool UnreadOnly = false)
        : IRequest<Response<IReadOnlyList<NotificationItemDto>>>;

    public sealed record GetUnreadNotificationsCountQuery()
        : IRequest<Response<int>>;
}
