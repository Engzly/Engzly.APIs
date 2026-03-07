using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    
    public class DeleteTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo, ICurrentUserService _currentUser): ResponseHandler , IRequestHandler<DeleteTaskCommand, Response<string>>
    {


        public async Task<Response<string>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = await _gigRepo.GetByIdAsync(request.Id, cancellationToken);
            if (gig == null)
            {
                return NotFound<string>("Task not found");
            }
            var currentUserId = _currentUser.GetCurrentUser().Id;
            if (gig.OwnerId != currentUserId)
            {
                return Unauthorized<string>();
            }

             _gigRepo.Delete(gig);
             await _gigRepo.CompleteAsync(cancellationToken);
             return Success(gig.Id, "Task deleted successfully");
        }
    }
}
