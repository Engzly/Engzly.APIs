using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public record DecideOnProposalCommand(
    string ProposalId,
    ProposalStatus Decision
) : IRequest<Response<DecideOnProposalResponse>>;
}
