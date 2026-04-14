using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Verification.Commands.Models
{
    public sealed record ApproveVerificationCommand(string VerificationId)
        : IRequest<Response<string>>;
}
