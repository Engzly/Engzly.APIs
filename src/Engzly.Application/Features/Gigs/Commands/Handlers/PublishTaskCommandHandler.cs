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
    using Engzly.Domain.Enums;
    using MediatR;

    public sealed class PublishTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo, ICurrentUserService _currentUser, IMapper _mapper) : ResponseHandler, IRequestHandler<PublishTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(PublishTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = _mapper.Map<Gig>(request);
            var now = DateTime.UtcNow;

            gig.OwnerId = _currentUser.GetCurrentUser().Id;
            gig.Status = GigStatus.Published;
            gig.CreatedOn = now;
            gig.LastModifiedOn = now;
            gig.CompletedOn = default;

            gig.Location = new Location(request.Latitude, request.Longitude);



            await _gigRepo.AddAsync(gig, cancellationToken);
            await _gigRepo.CompleteAsync(cancellationToken);
            return Success(gig.Id, "Task published successfully");

        }
    }
}
