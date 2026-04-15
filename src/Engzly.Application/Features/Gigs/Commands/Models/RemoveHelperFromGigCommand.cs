using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed record RemoveHelperFromGigCommand(
        string GigId,
        string HelperId,
        string Reason)
        : IRequest<Response<string>>;
}
