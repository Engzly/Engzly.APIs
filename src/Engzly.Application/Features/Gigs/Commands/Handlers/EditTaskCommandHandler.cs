using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Specifications.GigSpecifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{

    public sealed class EditTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo,
                                                IGenericRepository<Proposal, string> _proposalRepo,
                                                INotificationService _notificationService,
                                                ICurrentUserService _currentUser,
                                                UserManager<User> _userManager,
                                                IMapper _mapper) : ResponseHandler, IRequestHandler<EditTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(EditTaskCommand request, CancellationToken cancellationToken)
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

            _mapper.Map(request, gig);

            gig.LastModifiedOn = DateTime.UtcNow;
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


            // TODO : Notify Taskers who applied for this gig about the update

            //var proposals = await _proposalRepo.GetAllAsync(new ProposalsByGigIdSpec(gig.Id));
            //var taskerIds = proposals.Select(p => p.TaskerId).Distinct().ToList();

            //// جيب الـ device tokens بتاعتهم
            //var deviceTokens = await _userManager.GetDeviceTokensForUsers(taskerIds);

            //if (deviceTokens.Any())
            //{
            //    await _notificationService.SendNotificationAsync(
            //        deviceTokens,
            //        "Gig Updated",
            //        $"The gig '{gig.Title}' you applied for has been updated."
            //    );
            //}

            return Success(gig.Id, "Gig updated successfully");
        }


    }
}