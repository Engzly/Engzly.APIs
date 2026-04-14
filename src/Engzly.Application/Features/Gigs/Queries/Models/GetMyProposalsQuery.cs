using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record GetMyProposalsQuery()
        : IRequest<Response<IReadOnlyList<ProposalListItemResponse>>>;
}
