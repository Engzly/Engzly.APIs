using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Verification.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Application.Responses.VerificationResponses;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Verification.Commands.Handlers
{
    public sealed class SubmitVerificationCommandHandler(
        IGenericRepository<IdentityVerification, string> _repo,
        ICurrentUserService _currentUser,
        UserManager<User> _userManager,
        IFileService _fileService)
        : ResponseHandler, IRequestHandler<SubmitVerificationCommand, Response<SubmitVerificationResponse>>
    {
        public async Task<Response<SubmitVerificationResponse>> Handle(
            SubmitVerificationCommand request,
            CancellationToken cancellationToken)
        {
            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser is null || string.IsNullOrWhiteSpace(currentUser.Id))
                return Unauthorized<SubmitVerificationResponse>();

            var user = await _userManager.FindByIdAsync(currentUser.Id);
            if (user is null)
                return Unauthorized<SubmitVerificationResponse>("User not found");

            if (user.IsIdentityVerified)
                return Conflict<SubmitVerificationResponse>("Your identity is already verified");

            if (user.AccountType != AccountType.Helper)
                return Forbidden<SubmitVerificationResponse>("Only helpers submit identity verification");

            var existingActive = await _repo.GetAllAsync(
                new ActiveVerificationByUserSpec(currentUser.Id),
                cancellationToken);

            if (existingActive.Any())
                return Conflict<SubmitVerificationResponse>("You already have a verification in progress");

            string frontPath, backPath, selfiePath;
            try
            {
                var subFolder = $"verifications/{currentUser.Id}";
                frontPath = await _fileService.SavePrivateAsync(request.FrontImage, subFolder);
                backPath = await _fileService.SavePrivateAsync(request.BackImage, subFolder);
                selfiePath = await _fileService.SavePrivateAsync(request.Selfie, subFolder);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                return BadRequest<SubmitVerificationResponse>(ex.Message);
            }

            var verification = IdentityVerification.Create(currentUser.Id, frontPath, backPath, selfiePath);
            await _repo.AddAsync(verification, cancellationToken);
            await _repo.CompleteAsync(cancellationToken);

            return Created(new SubmitVerificationResponse
            {
                VerificationId = verification.Id,
                Status = verification.Status,
                SubmittedOn = verification.SubmittedOn
            });
        }

        private sealed class ActiveVerificationByUserSpec : BaseSpecification<IdentityVerification>
        {
            public ActiveVerificationByUserSpec(string userId)
                : base(v => v.UserId == userId && v.Status != VerificationStatus.Rejected)
            { }
        }
    }
}
