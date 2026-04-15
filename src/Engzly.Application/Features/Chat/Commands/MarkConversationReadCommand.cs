using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record MarkConversationReadCommand(string ConversationId)
        : IRequest<Response<int>>;

    public sealed class MarkConversationReadCommandHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ConversationParticipant, string> _participants,
        IGenericRepository<ChatMessage, string> _messages,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<MarkConversationReadCommand, Response<int>>
    {
        public async Task<Response<int>> Handle(
            MarkConversationReadCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<int>();

            var conversation = await _conversations.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<int>("Conversation not found");

            if (conversation.IsBot)
            {
                if (conversation.OwnerId != caller.Id)
                    return Forbidden<int>("Not your bot conversation");
            }
            else
            {
                var myParticipation = await _participants.GetAllAsync(
                    new UserParticipationSpec(conversation.Id, caller.Id),
                    cancellationToken);

                if (!myParticipation.Any(p => p.LeftOn == null))
                    return Forbidden<int>("You are not an active participant in this conversation");
            }

            var unread = await _messages.GetAllAsync(
                new UnreadIncomingMessagesSpec(conversation.Id, caller.Id),
                cancellationToken);

            if (unread.Count == 0)
                return Success(0, "No unread messages");

            foreach (var m in unread)
            {
                m.IsRead = true;
                _messages.Update(m);
            }

            await _messages.CompleteAsync(cancellationToken);
            return Success(unread.Count, "Messages marked as read");
        }

        private sealed class UnreadIncomingMessagesSpec : BaseSpecification<ChatMessage>
        {
            public UnreadIncomingMessagesSpec(string conversationId, string userId)
                : base(m => m.ConversationId == conversationId
                         && m.SenderId != userId
                         && !m.IsRead
                         && !m.IsDeleted)
            { }
        }
    }
}
