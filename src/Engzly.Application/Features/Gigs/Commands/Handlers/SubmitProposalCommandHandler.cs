using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public class SubmitProposalCommandHandler(IGenericRepository<Gig, string> _gigRepository, IGenericRepository<Proposal, string> _proposalRepository) : ResponseHandler, IRequestHandler<SubmitProposalCommand, Response<SubmitProposalResponse>>
    {


        public async Task<Response<SubmitProposalResponse>> Handle(SubmitProposalCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                return Unauthorized<SubmitProposalResponse>(" Must Login First ");
            var gig = await _gigRepository.GetByIdAsync(request.GigId, ct);
            if (gig is null)
                return NotFound<SubmitProposalResponse>("Task Not Found ");

            if (gig.OwnerId == request.CurrentUserId)
                return Forbidden<SubmitProposalResponse>("You Can't Applay For Your Task ");

            if (gig.Status != GigStatus.Published)
                return Gone<SubmitProposalResponse>($"Task In  {gig.Status} Status S You Can't Applay ");

            var spec = new ProposalByGigAndTaskerSpecification(request.GigId, request.CurrentUserId);
            if (await _proposalRepository.ExistsAsync(spec, ct))
                return Conflict<SubmitProposalResponse>("You can't Applay twice");

            var proposal = Proposal.Create(request.GigId, request.CurrentUserId, request.Message);
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
