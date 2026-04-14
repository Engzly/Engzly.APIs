using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.VerificationResponses;
using MediatR;

namespace Engzly.Application.Features.Verification.Queries.Models
{
    public sealed record GetVerificationDetailsQuery(string VerificationId)
        : IRequest<Response<VerificationDetailsResponse>>;
}
