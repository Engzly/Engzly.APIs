using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public class GetTaskDetailsQueryHandler(
        IGenericRepository<Gig, string> repository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<GetTaskDetailsQueryHandler> logger)
        : ResponseHandler,
          IRequestHandler<GetTaskDetailsQuery, Response<TaskDetailedResponse>>
    {
        public async Task<Response<TaskDetailedResponse>> Handle(
            GetTaskDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUser = currentUserService.GetCurrentUser();
            var currentUserId = currentUser.Id;

            logger.LogInformation(
                "Fetching task details. TaskId: {TaskId}, UserId: {UserId}",
                request.TaskId,
                currentUserId);

            var spec = new TaskDetailsSpecification(request.TaskId);
            var task = await repository.GetByIdAsync(
                request.TaskId,
                spec,
                cancellationToken);

            if (task == null)
            {
                logger.LogWarning(
                    "Task not found. TaskId: {TaskId}, RequestedBy: {UserId}",
                    request.TaskId,
                    currentUserId);

                return NotFound<TaskDetailedResponse>("Task not found");
            }

            var result = mapper.Map<TaskDetailedResponse>(task);

            result.MediaUrls = task.Medias.Select(x => x.Url).ToList();

            result.IsAppliedByMe =
                task.TaskersAssignments.Any(x => x.TaskerId == currentUserId);

            result.CurrentFilledCount =
                task.TaskersAssignments.Count;

            result.ClientInfo = task.Client != null
                ? new ClientInfoResponse
                {
                    UserId = task.Client.Id,
                    Name = $"{task.Client.FirstName} {task.Client.LastName}",
                    AvatarUrl = task.Client.ProfileImageUrl
                } : null;

            logger.LogInformation(
                "Task details retrieved successfully. TaskId: {TaskId}, FilledCount: {FilledCount}",
                request.TaskId,
                result.CurrentFilledCount);

            logger.LogDebug(
                "User application status checked. TaskId: {TaskId}, UserId: {UserId}, IsAppliedByMe: {IsApplied}",
                request.TaskId,
                currentUserId,
                result.IsAppliedByMe);


            return Success(result, "Task Details retrieved Successfully");
        }
    }
}