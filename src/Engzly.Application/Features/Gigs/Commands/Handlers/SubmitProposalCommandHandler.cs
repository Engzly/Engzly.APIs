using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public class SubmitProposalCommandHandler(IGenericRepository<Gig, string> _gigRepository, IGenericRepository<Proposal, string> _proposalRepository, ICurrentUserService _currentUser) : ResponseHandler, IRequestHandler<SubmitProposalCommand, Response<SubmitProposalResponse>>
    {


        public async Task<Response<SubmitProposalResponse>> Handle(SubmitProposalCommand request, CancellationToken ct)
        {
            var currentUser = _currentUser.GetCurrentUser();
            if (string.IsNullOrWhiteSpace(currentUser.Id))
                return Unauthorized<SubmitProposalResponse>(" Must Login First ");

            var gig = await _gigRepository.GetByIdAsync(request.GigId, ct);
            if (gig is null)
                return NotFound<SubmitProposalResponse>("Task Not Found ");

            if (gig.OwnerId == currentUser.Id)
                return Forbidden<SubmitProposalResponse>("You Can't Apply For Your Task ");

            if (gig.DueDate < DateTime.UtcNow)
                return Gone<SubmitProposalResponse>("Task Expired You Can't Apply ");

            if (gig.Status != GigStatus.Published)
                return Gone<SubmitProposalResponse>($"Task In  {gig.Status} Status S You Can't Apply ");

            var spec = new ProposalByGigAndTaskerSpecification(request.GigId, currentUser.Id);
            if (await _proposalRepository.ExistsAsync(spec, ct))
                return Conflict<SubmitProposalResponse>("You can't Apply twice");

            var proposal = Proposal.Create(request.GigId, currentUser.Id, request.Message);
            await _proposalRepository.AddAsync(proposal, ct);
            await _proposalRepository.CompleteAsync(ct);

            var _response = new SubmitProposalResponse(
          message: "Application submitted successfully.",
           applicationId: proposal.Id
        );

            return Created(_response);
        }
    }
}
