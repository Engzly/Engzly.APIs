using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using MediatR;


namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public class DecideOnProposalCommandHandler(IUnitOfWork _unitOfWork) : ResponseHandler, IRequestHandler<DecideOnProposalCommand, Response<DecideOnProposalResponse>>
    {

        public async Task<Response<DecideOnProposalResponse>> Handle(DecideOnProposalCommand request, CancellationToken ct)
        {
            var _proposalsRepo = _unitOfWork.Proposals;
            var _gigsRepo = _unitOfWork.Gigs;
            var _gigAssignmentRepo = _unitOfWork.GigAssignments;

            var proposal = await _unitOfWork.Proposals.GetByIdAsync(request.ProposalId, ct);
            if (proposal is null)
                return NotFound<DecideOnProposalResponse>($"اRequest with Id {request.ProposalId} Not Exist  ");

            var gig = await _gigsRepo.GetByIdLockedAsync(proposal.GigId, ct);
            if (gig is null)
                return NotFound<DecideOnProposalResponse>($"Task with Id {gig?.Id} Not Exist ");

            var currentUserId = request.CurrentUserId;
            if (currentUserId != gig.OwnerId)
                return Forbidden<DecideOnProposalResponse>("You Don't Have Permision to Make Any Changes on this Task ");

            if (gig.Status != GigStatus.Published)
                return Gone<DecideOnProposalResponse>($"Task is {gig.Status.ToString()}");


            if (proposal.Status != ProposalStatus.Pending)
                return Conflict<DecideOnProposalResponse>($"The Proposal is {proposal.Status.ToString()}");

            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {

                switch (request.Decision)
                {
                    case ProposalStatus.Approved:
                        proposal.Approve();
                        var assignment = new GigAssignment
                        {
                            Id = Guid.NewGuid().ToString(),
                            GigId = gig.Id,
                            TaskerId = proposal.TaskerId,
                            AssignedOn = DateTime.UtcNow,
                            ClientId = gig.OwnerId
                        };
                        await _gigAssignmentRepo.AddAsync(assignment);
                        await _gigAssignmentRepo.CompleteAsync(ct);
                        break;

                    case ProposalStatus.Rejected:
                        proposal.Reject();
                        break;

                    default:
                        return BadRequest<DecideOnProposalResponse>("You can't Apply This Decision You Must Choose between (Approved: 1 | Rejected: 2 ) ");
                }

                _proposalsRepo.Update(proposal);
                _gigsRepo.Update(gig);

                await _proposalsRepo.CompleteAsync(ct);
                await _gigsRepo.CompleteAsync(ct);


                await _unitOfWork.CommitTransactionAsync(ct);
                var res = new DecideOnProposalResponse(
                    Message: " congrats Approved on The Helper Request  ",
                    ProposalId: proposal.Id,
                    Decision: request.Decision,
                    TaskId: gig.Id,
                    TaskStatus: gig.Status.ToString(),
                    ClientId: gig.OwnerId,
                    TaskerId: proposal.TaskerId
                );
                return Success(res);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return InternalServerError<DecideOnProposalResponse>($" {ex.Message} ");
            }
        }
    }
}

