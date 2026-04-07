using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{

    public sealed record SubmitProposalCommand(
        string GigId,
        string Message
    ) : IRequest<Response<SubmitProposalResponse>>;
}
