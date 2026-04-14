using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Verification.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Verification.Commands.Handlers
{
    public sealed class RejectVerificationCommandHandler(
        IGenericRepository<IdentityVerification, string> _repo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<RejectVerificationCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            RejectVerificationCommand request,
            CancellationToken cancellationToken)
        {
            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser is null || string.IsNullOrWhiteSpace(currentUser.Id))
                return Unauthorized<string>();

            var verification = await _repo.GetByIdAsync(request.VerificationId, cancellationToken);
            if (verification is null)
                return NotFound<string>("Verification not found");

            if (verification.Status == VerificationStatus.Approved)
                return Conflict<string>("Cannot reject an already approved verification");

            if (verification.Status == VerificationStatus.Rejected)
                return Conflict<string>("Verification is already rejected");

            verification.Reject(currentUser.Id, request.Reason);

            _repo.Update(verification);
            await _repo.CompleteAsync(cancellationToken);

            return Success(verification.Id, "Verification rejected");
        }
    }
}
