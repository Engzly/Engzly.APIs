using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
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
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id) || caller.AccountType != AccountType.Client.ToString())
                Unauthorized<IReadOnlyList<TaskListItemResponse>>();
            var spec = new GigsByOwnerIdSpecification(caller.Id);
            var gigs = await gigRepo.GetAllAsync(spec, cancellationToken: cancellationToken);
            if (gigs.Count == 0)
                return NotFound<IReadOnlyList<TaskListItemResponse>>("No gigs found for the current client.");

            var response = gigs.Select(g => new TaskListItemResponse
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

            return Success<IReadOnlyList<TaskListItemResponse>>(response);
        }
    }
}
