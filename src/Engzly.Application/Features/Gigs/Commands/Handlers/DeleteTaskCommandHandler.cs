using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{

    public sealed class DeleteTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo, ICurrentUserService _currentUser) : ResponseHandler, IRequestHandler<DeleteTaskCommand, Response<string>>
    {

        public async Task<Response<string>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = await _gigRepo.GetByIdAsync(
                request.Id,
                new GigWithMediasByIdSpecification(),
                cancellationToken);

            if (gig == null)
            {
                return NotFound<string>("Task not found");
            }
            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized<string>();

            var currentUserId = currentUser.Id;
            if (gig.OwnerId != currentUserId)
            {
                return Unauthorized<string>();
            }

            _gigRepo.Delete(gig);
            await _gigRepo.CompleteAsync(cancellationToken);

            return Deleted<string>();
        }
    }
}
