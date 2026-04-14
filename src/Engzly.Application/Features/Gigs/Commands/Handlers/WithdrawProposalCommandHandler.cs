using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class WithdrawProposalCommandHandler(
        IGenericRepository<Proposal, string> _proposalRepo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<WithdrawProposalCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            WithdrawProposalCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<string>();

            var proposal = await _proposalRepo.GetByIdAsync(request.ProposalId, cancellationToken);
            if (proposal is null)
                return NotFound<string>("Proposal not found");

            if (proposal.TaskerId != caller.Id)
                return Forbidden<string>("You can only withdraw your own proposal");

            try
            {
                proposal.Withdraw();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict<string>(ex.Message);
            }

            _proposalRepo.Update(proposal);
            await _proposalRepo.CompleteAsync(cancellationToken);

            return Success(proposal.Id, "Proposal withdrawn");
        }
    }
}
