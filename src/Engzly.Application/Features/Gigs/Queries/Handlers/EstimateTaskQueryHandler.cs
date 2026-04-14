using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.AI;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public sealed class EstimateTaskQueryHandler(ITaskEstimator _estimator)
        : ResponseHandler, IRequestHandler<EstimateTaskQuery, Response<TaskEstimate>>
    {
        public async Task<Response<TaskEstimate>> Handle(
            EstimateTaskQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest<TaskEstimate>("Title is required");

            var estimate = await _estimator.EstimateAsync(
                new TaskEstimationInput(
                    request.Title,
                    request.Description ?? string.Empty,
                    request.CategoryId,
                    Math.Max(1, request.NumberOfTaskersNeeded)),
                cancellationToken);

            return Success(estimate);
        }
    }
}
