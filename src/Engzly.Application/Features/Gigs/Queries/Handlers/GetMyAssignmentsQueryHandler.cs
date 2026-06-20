using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public sealed class GetMyAssignmentsQueryHandler(
        IGenericRepository<GigAssignment, string> _repo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetMyAssignmentsQuery, Response<IReadOnlyList<AssignmentListItemResponse>>>
    {
        public async Task<Response<IReadOnlyList<AssignmentListItemResponse>>> Handle(
            GetMyAssignmentsQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<IReadOnlyList<AssignmentListItemResponse>>();

            //var role = (request.Role ?? "any").ToLowerInvariant();
            var userId = caller.Id;


            var spec = new AssignmentsByUserSpec(userId);
            //var spec = role switch
            //{
            //    "client" => new AssignmentsByUserSpec(userId, asClient: true),
            //    "helper" => new AssignmentsByUserSpec(userId, asClient: false),
            //    _ => new AssignmentsByUserSpec(userId, asClient: null)
            //};

            var assignments = await _repo.GetAllAsync(spec, cancellationToken);

            var items = assignments
                .OrderByDescending(a => a.AssignedOn)
                .Select(a => new AssignmentListItemResponse
                {
                    Id = a.Id,
                    GigId = a.GigId,
                    GigTitle = a.Gig?.Title,
                    ClientId = a.ClientId,
                    ClientFullName = a.Client is null ? null : $"{a.Client.FirstName} {a.Client.LastName}".Trim(),
                    TaskerId = a.TaskerId,
                    TaskerFullName = a.Tasker is null ? null : $"{a.Tasker.FirstName} {a.Tasker.LastName}".Trim(),
                    AssignedOn = a.AssignedOn,
                    IsCompletedByTasker = a.IsCompletedByTasker,
                    CompletedByTaskerOn = a.CompletedByTaskerOn
                })
                .ToList();

            return Success<IReadOnlyList<AssignmentListItemResponse>>(items);
        }

        private sealed class AssignmentsByUserSpec : BaseSpecification<GigAssignment>
        {
            public AssignmentsByUserSpec(string userId/*, bool? asClient*/)
                : base(a => /*asClient == true*/
                        //? a.ClientId == userId
                        //: asClient == false ?
                        a.TaskerId == userId
                       || a.ClientId == userId /*|| a.TaskerId == userId*/)
            {
                AddInclude(a => a.Gig);
                AddInclude(a => a.Client);
                AddInclude(a => a.Tasker);
            }
        }
    }
}
