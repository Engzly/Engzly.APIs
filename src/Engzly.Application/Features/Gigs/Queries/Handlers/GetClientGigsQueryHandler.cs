using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications.GigSpecifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public class GetClientGigsQueryHandler(
        IGenericRepository<Gig, string> gigRepo,
        ICurrentUserService _currentUser
        )
        : ResponseHandler, IRequestHandler<GetClientGigsQuery, Response<IReadOnlyList<TaskListItemResponse>>>
    {
        public async Task<Response<IReadOnlyList<TaskListItemResponse>>> Handle(GetClientGigsQuery request, CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<IReadOnlyList<TaskListItemResponse>>();
            var gigs = await gigRepo.GetAllAsync(
                new GigsByUserSpecification(caller.Id),
                cancellationToken);
            if (gigs.Count == 0)
                return Success<IReadOnlyList<TaskListItemResponse>>([]);

            var filtered = gigs.AsEnumerable();

            if (request.Status.HasValue)
                filtered = filtered.Where(
                    g => g.Status == request.Status.Value);

            var ordered = filtered
                .OrderByDescending(g => g.CreatedOn)
                .ToList();

            var total = ordered.Count;

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new TaskListItemResponse
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name,
                    Budget = g.Budget,
                    NumberOfTaskersNeeded = g.NumberOfTaskersNeeded,
                    Status = g.Status,
                    StartDate = g.StartDate,
                    DueDate = g.DueDate,
                    CreatedOn = g.CreatedOn,
                    Latitude = g.Location?.Latitude ?? 0,
                    Longitude = g.Location?.Longitude ?? 0,
                    OwnerId = g.OwnerId
                }).ToList();

            var response = Success<IReadOnlyList<TaskListItemResponse>>(items);

            response.Meta = new
            {
                total,
                page,
                pageSize
            };

            return response;
        }
    }
}
