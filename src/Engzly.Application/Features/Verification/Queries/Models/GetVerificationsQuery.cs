using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.VerificationResponses;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Verification.Queries.Models
{
    public sealed record GetVerificationsQuery(
        VerificationStatus? Status = null,
        int Page = 1,
        int PageSize = 20)
        : IRequest<Response<IReadOnlyList<VerificationListItemResponse>>>;
}
