using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class PublishTaskCommandHandler
        : ResponseHandler,
          IRequestHandler<PublishTaskCommand, Response<string>>
    {
        private readonly IGenericRepository<Gig, string> _gigRepo;
        private readonly IGenericRepository<Media, Guid> _mediaRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public PublishTaskCommandHandler(
            IGenericRepository<Gig, string> gigRepo,
            IGenericRepository<Media, Guid> mediaRepo,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _gigRepo = gigRepo;
            _mediaRepo = mediaRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(
            PublishTaskCommand request,
            CancellationToken cancellationToken)
        {
            var gig = _mapper.Map<Gig>(request);

            var now = DateTime.UtcNow;

            var currentUser = _currentUser.GetCurrentUser();

            if (currentUser == null)
                return Unauthorized<string>();

           
            gig.Id = Guid.NewGuid().ToString();

            gig.OwnerId = currentUser.Id;
            gig.Status = GigStatus.Published;
            gig.CreatedOn = now;
            gig.LastModifiedOn = now;
            gig.CompletedOn = default;

            gig.Location = new Location(request.Latitude, request.Longitude);


            if (request.MediaIds != null && request.MediaIds.Any())
            {
                var uploadedMedias = new List<Media>();

                foreach (var mediaId in request.MediaIds)
                {
                    var media = await _mediaRepo.GetByIdAsync(mediaId, cancellationToken);
                    if (media == null)
                        return BadRequest<string>($"Media {mediaId} not found");

                    media.IsTemp = false;
                    media.GigId = gig.Id;

                    _mediaRepo.Update(media);

                    uploadedMedias.Add(media);
                }

                gig.Medias = uploadedMedias.ToHashSet();
            }

            await _gigRepo.AddAsync(gig, cancellationToken);

            await _gigRepo.CompleteAsync(cancellationToken);

            return Success(gig.Id, "Task published successfully");
        }
    }
}