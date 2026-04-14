using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Verification.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.VerificationResponses;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Verification.Queries.Handlers
{
    public sealed class GetVerificationsQueryHandler(
        IGenericRepository<IdentityVerification, string> _repo)
        : ResponseHandler, IRequestHandler<GetVerificationsQuery, Response<IReadOnlyList<VerificationListItemResponse>>>
    {
        public async Task<Response<IReadOnlyList<VerificationListItemResponse>>> Handle(
            GetVerificationsQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new VerificationListSpec(request.Status);
            var all = await _repo.GetAllAsync(spec, cancellationToken);

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = all
                .OrderByDescending(v => v.SubmittedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VerificationListItemResponse
                {
                    VerificationId = v.Id,
                    UserId = v.UserId,
                    UserFullName = v.User is null ? string.Empty : $"{v.User.FirstName} {v.User.LastName}".Trim(),
                    Status = v.Status,
                    SubmittedOn = v.SubmittedOn,
                    ReviewedOn = v.ReviewedOn
                })
                .ToList();

            return Success<IReadOnlyList<VerificationListItemResponse>>(items);
        }

        private sealed class VerificationListSpec : BaseSpecification<IdentityVerification>
        {
            public VerificationListSpec(Domain.Enums.VerificationStatus? status)
                : base(v => status == null || v.Status == status)
            {
                AddInclude(v => v.User);
            }
        }
    }
}
