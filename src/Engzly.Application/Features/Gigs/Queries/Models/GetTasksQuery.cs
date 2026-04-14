using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record GetTasksQuery(
        string? Search = null,
        string? CategoryId = null,
        GigStatus? Status = null,
        decimal? MinBudget = null,
        decimal? MaxBudget = null,
        DateTime? StartDateFrom = null,
        DateTime? DueDateTo = null,
        int Page = 1,
        int PageSize = 20)
        : IRequest<Response<IReadOnlyList<TaskListItemResponse>>>;
}
