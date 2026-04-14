using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Verification.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.VerificationResponses;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Verification.Queries.Handlers
{
    public sealed class GetVerificationDetailsQueryHandler(
        IGenericRepository<IdentityVerification, string> _repo)
        : ResponseHandler, IRequestHandler<GetVerificationDetailsQuery, Response<VerificationDetailsResponse>>
    {
        public async Task<Response<VerificationDetailsResponse>> Handle(
            GetVerificationDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var verification = await _repo.GetByIdAsync(
                request.VerificationId,
                new VerificationWithUserSpec(request.VerificationId),
                cancellationToken);

            if (verification is null)
                return NotFound<VerificationDetailsResponse>("Verification not found");

            var response = new VerificationDetailsResponse
            {
                VerificationId = verification.Id,
                UserId = verification.UserId,
                UserFullName = verification.User is null ? string.Empty : $"{verification.User.FirstName} {verification.User.LastName}".Trim(),
                UserEmail = verification.User?.Email ?? string.Empty,
                Status = verification.Status,
                SubmittedOn = verification.SubmittedOn,
                ReviewedOn = verification.ReviewedOn,
                ReviewedByAdminId = verification.ReviewedByAdminId,
                RejectionReason = verification.RejectionReason,
                FrontImageUrl = $"/api/verification/file/{verification.Id}/front",
                BackImageUrl = $"/api/verification/file/{verification.Id}/back",
                SelfieUrl = $"/api/verification/file/{verification.Id}/selfie"
            };

            return Success(response);
        }

        private sealed class VerificationWithUserSpec : BaseSpecification<IdentityVerification>
        {
            public VerificationWithUserSpec(string id)
                : base(v => v.Id == id)
            {
                AddInclude(v => v.User);
            }
        }
    }
}
