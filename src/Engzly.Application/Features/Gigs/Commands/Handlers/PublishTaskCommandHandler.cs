namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Engzly.Application.Common.Bases;
    using Engzly.Application.Features.Gigs.Commands.Models;
    using Engzly.Application.Interfaces.AI;
    using Engzly.Application.Interfaces.Authentication;
    using Engzly.Application.Interfaces.Repositories;
    using Engzly.Application.Responses.GigsResponse;
    using Engzly.Domain.Entities.Common;
    using Engzly.Domain.Entities.Gigs;
    using Engzly.Domain.Entities.Identity;
    using Engzly.Domain.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;

    public sealed class PublishTaskCommandHandler(
        IGenericRepository<Gig, string> _gigRepo,
        ICurrentUserService _currentUser,
        UserManager<User> _userManger,
        IMapper _mapper,
        IGenericRepository<Media, Guid> _mediaRepo,
        ICategoryClassifier _classifier,
        IOptions<CategoryAIOptions> _categoryAiOptions)
        : ResponseHandler, IRequestHandler<PublishTaskCommand, Response<PublishGigResponse>>
    {
        public async Task<Response<PublishGigResponse>> Handle(PublishTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = _mapper.Map<Gig>(request);

            var now = DateTime.UtcNow;

            var currentUser = _currentUser.GetCurrentUser();

            if (currentUser == null)
                return Unauthorized<PublishGigResponse>();

            var _user = await _userManger.FindByIdAsync(currentUser.Id);

            if (_user.AccountType != AccountType.Client)
                return Unauthorized<PublishGigResponse>("You an Authorized TO Publish Task You Must Create Client Account ");

            if (string.IsNullOrWhiteSpace(request.CategoryId))
            {
                var suggestions = await _classifier.ClassifyAsync(
                    request.Title,
                    request.Description,
                    topK: 1,
                    cancellationToken);

                var top = suggestions.FirstOrDefault();
                var minConfidence = _categoryAiOptions.Value.MinConfidence;

                if (top is null || top.Score < minConfidence)
                    return BadRequest<PublishGigResponse>("Could not auto-detect category, please pick one");

                gig.CategoryId = top.CategoryId;
            }

            gig.Id = Guid.NewGuid().ToString();

            gig.OwnerId = currentUser.Id;
            gig.Status = GigStatus.Published;
            gig.CreatedOn = now;
            gig.LastModifiedOn = now;
            gig.CompletedOn = default;

            gig.Location = new Location(request.Latitude, request.Longitude);


            if (request.MediaIds?.Any() == true)
            {
                var uploadedMedias = new List<Media>();

                foreach (var mediaId in request.MediaIds)
                {
                    var media = await _mediaRepo.GetByIdAsync(mediaId, cancellationToken);
                    if (media == null)
                        return BadRequest<PublishGigResponse>($"Media {mediaId} not found");

                    media.IsTemp = false;
                    media.GigId = gig.Id;

                    _mediaRepo.Update(media);

                    uploadedMedias.Add(media);
                }

                gig.Medias = uploadedMedias.ToHashSet();
            }

            await _gigRepo.AddAsync(gig, cancellationToken);

            await _gigRepo.CompleteAsync(cancellationToken);


            var response = new PublishGigResponse
            (
                GigId: gig.Id,
                Title: gig.Title,
                Description: gig.Description,
                TaskStartDate: gig.StartDate.ToString("yyyy-MM-dd"),
                DueDate: gig.DueDate.ToString("yyyy-MM-dd"),
                Budget: gig.Budget,
                ClientName: _user.UserName
            );

            return Success(response, "Task published successfully");
        }
    }
}