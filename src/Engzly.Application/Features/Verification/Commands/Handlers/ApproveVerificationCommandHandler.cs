using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Verification.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Verification.Commands.Handlers
{
    public sealed class ApproveVerificationCommandHandler(
        IGenericRepository<IdentityVerification, string> _repo,
        UserManager<User> _userManager,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<ApproveVerificationCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            ApproveVerificationCommand request,
            CancellationToken cancellationToken)
        {
            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser is null || string.IsNullOrWhiteSpace(currentUser.Id))
                return Unauthorized<string>();

            var verification = await _repo.GetByIdAsync(request.VerificationId, cancellationToken);
            if (verification is null)
                return NotFound<string>("Verification not found");

            if (verification.Status == VerificationStatus.Approved)
                return Conflict<string>("Verification is already approved");

            if (verification.Status == VerificationStatus.Rejected)
                return Conflict<string>("Cannot approve a rejected verification");

            var targetUser = await _userManager.FindByIdAsync(verification.UserId);
            if (targetUser is null)
                return NotFound<string>("Target user not found");

            verification.Approve(currentUser.Id);
            targetUser.IsIdentityVerified = true;

            _repo.Update(verification);
            await _repo.CompleteAsync(cancellationToken);
            await _userManager.UpdateAsync(targetUser);

            return Success(verification.Id, "Verification approved");
        }
    }
}
