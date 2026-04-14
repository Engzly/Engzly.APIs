using Engzly.Application.Common.Bases;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Admin.Users.Queries
{
    public sealed record AdminUserListItemDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string City,
        AccountType AccountType,
        UserStatus Status,
        bool IsIdentityVerified,
        bool EmailConfirmed);

    public sealed record GetUsersQuery(
        string? Search = null,
        AccountType? AccountType = null,
        UserStatus? Status = null,
        bool? IsIdentityVerified = null,
        int Page = 1,
        int PageSize = 20)
        : IRequest<Response<IReadOnlyList<AdminUserListItemDto>>>;

    public sealed record GetUserByIdQuery(string UserId)
        : IRequest<Response<AdminUserListItemDto>>;
}
