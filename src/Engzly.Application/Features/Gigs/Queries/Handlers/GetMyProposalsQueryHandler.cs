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
    public sealed class GetMyProposalsQueryHandler(
        IGenericRepository<Proposal, string> _proposalRepo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetMyProposalsQuery, Response<IReadOnlyList<ProposalListItemResponse>>>
    {
        public async Task<Response<IReadOnlyList<ProposalListItemResponse>>> Handle(
            GetMyProposalsQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<IReadOnlyList<ProposalListItemResponse>>();

            var proposals = await _proposalRepo.GetAllAsync(
                new MyProposalsSpec(caller.Id),
                cancellationToken);

            var items = proposals
                .OrderByDescending(p => p.SubmittedOn)
                .Select(p => new ProposalListItemResponse
                {
                    Id = p.Id,
                    GigId = p.GigId,
                    GigTitle = p.Gig?.Title,
                    TaskerId = p.TaskerId,
                    TaskerFullName = null,
                    Message = p.Message,
                    Status = p.Status,
                    SubmittedOn = p.SubmittedOn
                })
                .ToList();

            return Success<IReadOnlyList<ProposalListItemResponse>>(items);
        }

        private sealed class MyProposalsSpec : BaseSpecification<Proposal>
        {
            public MyProposalsSpec(string taskerId) : base(p => p.TaskerId == taskerId)
            {
                AddInclude(p => p.Gig);
            }
        }
    }
}
