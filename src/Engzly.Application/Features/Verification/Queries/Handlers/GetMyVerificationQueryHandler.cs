using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Verification.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.VerificationResponses;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Verification.Queries.Handlers
{
    public sealed class GetMyVerificationQueryHandler(
        IGenericRepository<IdentityVerification, string> _repo,
        ICurrentUserService _currentUser,
        UserManager<User> _userManager)
        : ResponseHandler, IRequestHandler<GetMyVerificationQuery, Response<MyVerificationResponse>>
    {
        public async Task<Response<MyVerificationResponse>> Handle(
            GetMyVerificationQuery request,
            CancellationToken cancellationToken)
        {
            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser is null || string.IsNullOrWhiteSpace(currentUser.Id))
                return Unauthorized<MyVerificationResponse>();

            var user = await _userManager.FindByIdAsync(currentUser.Id);
            if (user is null)
                return Unauthorized<MyVerificationResponse>("User not found");

            var latest = (await _repo.GetAllAsync(
                new LatestVerificationByUserSpec(currentUser.Id),
                cancellationToken))
                .OrderByDescending(v => v.SubmittedOn)
                .FirstOrDefault();

            var response = new MyVerificationResponse
            {
                IsVerified = user.IsIdentityVerified,
                VerificationId = latest?.Id,
                Status = latest?.Status,
                SubmittedOn = latest?.SubmittedOn,
                ReviewedOn = latest?.ReviewedOn,
                RejectionReason = latest?.RejectionReason
            };

            return Success(response);
        }

        private sealed class LatestVerificationByUserSpec : BaseSpecification<IdentityVerification>
        {
            public LatestVerificationByUserSpec(string userId)
                : base(v => v.UserId == userId)
            { }
        }
    }
}
