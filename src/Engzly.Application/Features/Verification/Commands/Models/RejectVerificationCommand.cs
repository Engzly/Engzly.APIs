using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Verification.Commands.Models
{
    public sealed record RejectVerificationCommand(string VerificationId, string Reason)
        : IRequest<Response<string>>;
}
