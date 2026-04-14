using Engzly.Application.Common.Bases;
using Engzly.Application.Interfaces.AI;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record EstimateTaskQuery(
        string Title,
        string? Description,
        string? CategoryId,
        int NumberOfTaskersNeeded = 1)
        : IRequest<Response<TaskEstimate>>;
}
