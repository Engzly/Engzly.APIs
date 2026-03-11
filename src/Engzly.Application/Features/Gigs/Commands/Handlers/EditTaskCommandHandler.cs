using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{

    public sealed class EditTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo, ICurrentUserService _currentUser, IMapper _mapper) : ResponseHandler, IRequestHandler<EditTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(EditTaskCommand request, CancellationToken cancellationToken)
        {
   var gig = await _gigRepo.GetByIdAsync(
                request.Id,
                new GigWithMediasByIdSpecification(),
                cancellationToken);    
                
         if(gig == null )
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

            _mapper.Map(request, gig);

             gig.LastModifiedOn = DateTime.UtcNow ;
             gig.Location = new Location(request.Latitude, request.Longitude);
               gig.Medias = request.MediaUrls
                .Select(url => new Media
                {
                    Url = url,
                    IsTemp = false
                })
                .ToHashSet();

           _gigRepo.Update(gig); 
           await _gigRepo.CompleteAsync(cancellationToken);
           return Success(gig.Id, "Gig updated successfully");
 


    }


}
}