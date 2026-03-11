using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{

    public sealed class VerifyTaskCommandHandler(
            IGenericRepository<Gig, string> _gigRepo,
            ICurrentUserService _currentUser)
            : ResponseHandler, IRequestHandler<VerifyTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(VerifyTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = await _gigRepo.GetByIdAsync(request.Id, new GigWithAssignmentsByIdSpecification(), cancellationToken);
            if (gig == null)
                return NotFound<string>("Task not found");

            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized<string>();

            var currentUserId = currentUser.Id;
            if (gig.OwnerId != currentUserId)
                return Unauthorized<string>();

            if (gig.Status != GigStatus.PendingVerification)
                return BadRequest<string>("Task must be pending verification before it can be verified.");

            gig.Status = GigStatus.Completed;
            gig.CompletedOn = DateTime.UtcNow;
            gig.LastModifiedOn = DateTime.UtcNow;

            _gigRepo.Update(gig);
            await _gigRepo.CompleteAsync(cancellationToken);

            return Success(gig.Id, "Task verified and completed successfully.");
            

        }
    }
}