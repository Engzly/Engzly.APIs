namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Engzly.Application.Common.Bases;
    using Engzly.Application.Features.Gigs.Commands.Models;
    using Engzly.Application.Interfaces.Authentication;
    using Engzly.Application.Interfaces.Repositories;
    using Engzly.Domain.Entities.Common;
    using Engzly.Domain.Entities.Gigs;
    using Engzly.Domain.Entities.Identity;
    using Engzly.Domain.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Identity;

    public sealed class PublishTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo, ICurrentUserService _currentUser, UserManager<User> _userManger, IMapper _mapper) : ResponseHandler, IRequestHandler<PublishTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(PublishTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = _mapper.Map<Gig>(request);
            var now = DateTime.UtcNow;

            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized<string>();

            var _user = await _userManger.FindByIdAsync(currentUser.Id);

            if (_user.AccountType != AccountType.Client)
                return Unauthorized<string>("You an Authorized TO Publish Task You Must Create Client Account ");

            gig.OwnerId = currentUser.Id;
            gig.Status = GigStatus.Published;
            gig.CreatedOn = now;
            gig.LastModifiedOn = now;
            gig.CompletedOn = default;
            gig.Medias = request.MediaUrls?
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => new Media
                {
                    Url = url,
                    IsTemp = false
                })
                .ToHashSet() ?? new HashSet<Media>();

            gig.Location = new Location(request.Latitude, request.Longitude);



            await _gigRepo.AddAsync(gig, cancellationToken);
            await _gigRepo.CompleteAsync(cancellationToken);
            return Success(gig.Id, "Task published successfully");

        }
    }
}
