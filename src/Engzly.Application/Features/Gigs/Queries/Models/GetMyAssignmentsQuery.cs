using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record GetMyAssignmentsQuery(/*string? Role = null*/)
        : IRequest<Response<IReadOnlyList<AssignmentListItemResponse>>>;
}
