using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using MediatR;
using Microsoft.VisualBasic;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{

class  EditTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo, ICurrentUserService _currentUser, IMapper _mapper) : ResponseHandler, IRequestHandler<EditTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(EditTaskCommand request, CancellationToken cancellationToken)
        {
         var gig = await _gigRepo.GetByIdAsync(request.Id, cancellationToken); 
         if(gig == null )
            {
                return NotFound<string>("Task not found");
            }
            var currentUserId = _currentUser.GetCurrentUser().Id;
            if (gig.OwnerId != currentUserId)
            {
                return Unauthorized<string>();
            }

            _mapper.Map(request, gig);

             gig.LastModifiedOn = DateTime.UtcNow ;
             gig.Location = new Location(request.Latitude, request.Longitude);

           _gigRepo.Update(gig); 
           await _gigRepo.CompleteAsync(cancellationToken);
           return Success(gig.Id, "Task updated successfully");
 


    }


}
}