using System.Security.Claims;
using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Interfaces.Specifications;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public class GetTaskDetailsQueryHandler(
        IGenericRepository<Gig> repository,
        IMapper mapper,
        IHttpContextAccessor httpContext)
        : ResponseHandler,  
          IRequestHandler<GetTaskDetailsQuery, Response<TaskDetailedResponse>>
    {
        public async Task<Response<TaskDetailedResponse>> Handle(
            GetTaskDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new TaskDetailsSpecification(request.TaskId);
            var task = await repository.FirstOrDefaultAsync(spec, cancellationToken);

            if (task == null)
                return NotFound<TaskDetailedResponse>("Task not found"); 

            var currentUserId = httpContext.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = mapper.Map<TaskDetailedResponse>(task);
            result.IsAppliedByMe = task.Taskers.Any(x => x.Id == currentUserId);
            result.CurrentFilledCount = task.Taskers.Count;

            return Success(result); 
        }
    }
}