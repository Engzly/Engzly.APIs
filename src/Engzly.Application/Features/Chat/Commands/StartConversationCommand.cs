using Engzly.Application.Common.Bases;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record StartConversationCommand(string OtherUserId, string? GigId)
        : IRequest<Response<string>>;

    public sealed class StartConversationCommandHandler(
        IGenericRepository<Conversation, string> _repo,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<StartConversationCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            StartConversationCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<string>();

            if (string.IsNullOrWhiteSpace(request.OtherUserId))
                return BadRequest<string>("otherUserId is required");

            if (request.OtherUserId == caller.Id)
                return BadRequest<string>("Cannot start a conversation with yourself");

            var existing = await _repo.GetAllAsync(
                new ConversationBetweenUsersSpec(caller.Id, request.OtherUserId, request.GigId),
                cancellationToken);

            var conversation = existing.FirstOrDefault();
            if (conversation is not null)
                return Success(conversation.Id, "Existing conversation returned");

            conversation = new Conversation
            {
                Id = Guid.NewGuid().ToString(),
                UserAId = caller.Id,
                UserBId = request.OtherUserId,
                GigId = request.GigId,
                IsBot = false,
                CreatedOn = DateTime.UtcNow,
                LastMessageOn = DateTime.UtcNow
            };

            await _repo.AddAsync(conversation, cancellationToken);
            await _repo.CompleteAsync(cancellationToken);

            return Created(conversation.Id);
        }

        private sealed class ConversationBetweenUsersSpec : BaseSpecification<Conversation>
        {
            public ConversationBetweenUsersSpec(string userId, string otherUserId, string? gigId)
                : base(c =>
                    c.IsBot == false &&
                    ((c.UserAId == userId && c.UserBId == otherUserId) ||
                     (c.UserAId == otherUserId && c.UserBId == userId)) &&
                    (gigId == null || c.GigId == gigId))
            { }
        }
    }
}
