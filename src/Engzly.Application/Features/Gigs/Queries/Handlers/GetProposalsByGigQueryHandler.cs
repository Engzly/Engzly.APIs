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
    public sealed class GetProposalsByGigQueryHandler(
        IGenericRepository<Gig, string> _gigRepo,
        IGenericRepository<Proposal, string> _proposalRepo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetProposalsByGigQuery, Response<IReadOnlyList<ProposalListItemResponse>>>
    {
        public async Task<Response<IReadOnlyList<ProposalListItemResponse>>> Handle(
            GetProposalsByGigQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<IReadOnlyList<ProposalListItemResponse>>();

            var gig = await _gigRepo.GetByIdAsync(request.GigId, cancellationToken);
            if (gig is null)
                return NotFound<IReadOnlyList<ProposalListItemResponse>>("Task not found");

            if (gig.OwnerId != caller.Id)
                return Forbidden<IReadOnlyList<ProposalListItemResponse>>("Only the task owner can see proposals");

            var proposals = await _proposalRepo.GetAllAsync(
                new ProposalsByGigSpec(request.GigId),
                cancellationToken);

            var items = proposals
                .OrderByDescending(p => p.SubmittedOn)
                .Select(p => new ProposalListItemResponse
                {
                    Id = p.Id,
                    GigId = p.GigId,
                    GigTitle = p.Gig?.Title,
                    TaskerId = p.TaskerId,
                    TaskerFullName = p.Tasker is null ? null : $"{p.Tasker.FirstName} {p.Tasker.LastName}".Trim(),
                    Message = p.Message,
                    Status = p.Status,
                    SubmittedOn = p.SubmittedOn
                })
                .ToList();

            return Success<IReadOnlyList<ProposalListItemResponse>>(items);
        }

        private sealed class ProposalsByGigSpec : BaseSpecification<Proposal>
        {
            public ProposalsByGigSpec(string gigId) : base(p => p.GigId == gigId)
            {
                AddInclude(p => p.Tasker);
                AddInclude(p => p.Gig);
            }
        }
    }
}
