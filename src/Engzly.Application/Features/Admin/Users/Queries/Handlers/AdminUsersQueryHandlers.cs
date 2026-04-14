using Engzly.Application.Common.Bases;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Admin.Users.Queries.Handlers
{
    public sealed class GetUsersQueryHandler(
        UserManager<User> _userManager)
        : ResponseHandler, IRequestHandler<GetUsersQuery, Response<IReadOnlyList<AdminUserListItemDto>>>
    {
        public async Task<Response<IReadOnlyList<AdminUserListItemDto>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim();
                query = query.Where(u =>
                    u.Email!.Contains(s) ||
                    u.FirstName.Contains(s) ||
                    u.LastName.Contains(s));
            }

            if (request.AccountType.HasValue)
                query = query.Where(u => u.AccountType == request.AccountType.Value);

            if (request.Status.HasValue)
                query = query.Where(u => u.Status == request.Status.Value);

            if (request.IsIdentityVerified.HasValue)
                query = query.Where(u => u.IsIdentityVerified == request.IsIdentityVerified.Value);

            var materialized = query.ToList();
            var total = materialized.Count;

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            var users = materialized
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = users
                .Select(u => new AdminUserListItemDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.FirstName,
                    u.LastName,
                    u.City,
                    u.AccountType,
                    u.Status,
                    u.IsIdentityVerified,
                    u.EmailConfirmed))
                .ToList();

            var response = Success<IReadOnlyList<AdminUserListItemDto>>(items);
            response.Meta = new { total, page, pageSize };
            return await Task.FromResult(response);
        }
    }

    public sealed class GetUserByIdQueryHandler(
        UserManager<User> _userManager)
        : ResponseHandler, IRequestHandler<GetUserByIdQuery, Response<AdminUserListItemDto>>
    {
        public async Task<Response<AdminUserListItemDto>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                return NotFound<AdminUserListItemDto>("User not found");

            var dto = new AdminUserListItemDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.City,
                user.AccountType,
                user.Status,
                user.IsIdentityVerified,
                user.EmailConfirmed);

            return Success(dto);
        }
    }
}
