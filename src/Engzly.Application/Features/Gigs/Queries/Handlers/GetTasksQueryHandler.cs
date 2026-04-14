using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public sealed class GetTasksQueryHandler(IGenericRepository<Gig, string> _repo)
        : ResponseHandler, IRequestHandler<GetTasksQuery, Response<IReadOnlyList<TaskListItemResponse>>>
    {
        public async Task<Response<IReadOnlyList<TaskListItemResponse>>> Handle(
            GetTasksQuery request,
            CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllAsync(new TaskSearchSpecification(), cancellationToken);

            var filtered = all.AsEnumerable();

            if (request.Status.HasValue)
                filtered = filtered.Where(g => g.Status == request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.CategoryId))
                filtered = filtered.Where(g => g.CategoryId == request.CategoryId);

            if (request.MinBudget.HasValue)
                filtered = filtered.Where(g => g.Budget >= request.MinBudget.Value);

            if (request.MaxBudget.HasValue)
                filtered = filtered.Where(g => g.Budget <= request.MaxBudget.Value);

            if (request.StartDateFrom.HasValue)
                filtered = filtered.Where(g => g.StartDate >= request.StartDateFrom.Value);

            if (request.DueDateTo.HasValue)
                filtered = filtered.Where(g => g.DueDate <= request.DueDateTo.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                filtered = filtered.Where(g =>
                    g.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    g.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = filtered.OrderByDescending(g => g.CreatedOn).ToList();
            var total = ordered.Count;
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new TaskListItemResponse
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name,
                    Budget = g.Budget,
                    NumberOfTaskersNeeded = g.NumberOfTaskersNeeded,
                    Status = g.Status,
                    StartDate = g.StartDate,
                    DueDate = g.DueDate,
                    CreatedOn = g.CreatedOn,
                    Latitude = g.Location?.Latitude ?? 0,
                    Longitude = g.Location?.Longitude ?? 0,
                    OwnerId = g.OwnerId
                })
                .ToList();

            var response = Success<IReadOnlyList<TaskListItemResponse>>(items);
            response.Meta = new { total, page, pageSize };
            return response;
        }
    }
}
