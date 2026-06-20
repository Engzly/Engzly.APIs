using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications.GigSpecifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class CompleteTaskCommandHandler(
            IGenericRepository<Gig, string> _gigRepo,
            ICurrentUserService _currentUser)
            : ResponseHandler, IRequestHandler<CompleteTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = await _gigRepo.GetByIdAsync(request.GigId, new GigWithAssignmentsByIdSpecification(), cancellationToken);

            if (gig is null)
                return NotFound<string>("Task not found");

            var currentUserId = _currentUser.GetCurrentUser().Id;

            var assignment = gig.TaskersAssignments.FirstOrDefault(a => a.TaskerId == currentUserId);

            if (assignment is null)
            {
                return Unauthorized<string>("You are not assigned to this task.");
            }

            if (gig.OwnerId == currentUserId)
                return Unauthorized<string>("Owner cannot complete the task.");

            if (gig.Status != GigStatus.InProgress)
                return BadRequest<string>("Only tasks in progress can be marked as complete.");

            if (assignment.IsCompletedByTasker)
                return BadRequest<string>("You have already marked this task as complete.");

            assignment.IsCompletedByTasker = true;
            assignment.CompletedByTaskerOn = DateTime.UtcNow;

            // Check if all taskers have completed the task
            var AllTaskersCompleted = gig.TaskersAssignments.All(a => a.IsCompletedByTasker == true);

            if (AllTaskersCompleted)
                gig.Status = GigStatus.PendingVerification;

            _gigRepo.Update(gig);
            await _gigRepo.CompleteAsync(cancellationToken);
            var message = AllTaskersCompleted ? "Task completed and pending verification." : "Task marked as completed by you. Waiting for other taskers to complete.";

            return Success(gig.Id, message);
        }
    }




}


