using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record GetClientGigsQuery(GigStatus? Status,
    int Page = 1,
    int PageSize = 20)
    : IRequest<Response<IReadOnlyList<TaskListItemResponse>>>;
}
