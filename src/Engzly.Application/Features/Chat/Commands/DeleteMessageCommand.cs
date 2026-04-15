using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using MediatR;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record DeleteMessageCommand(string MessageId)
        : IRequest<Response<string>>;

    public sealed class DeleteMessageCommandHandler(
        IGenericRepository<ChatMessage, string> _messages,
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ConversationParticipant, string> _participants,
        ICurrentUserService _currentUser,
        IChatNotifier _notifier)
        : ResponseHandler, IRequestHandler<DeleteMessageCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            DeleteMessageCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<string>();

            var message = await _messages.GetByIdAsync(request.MessageId, cancellationToken);
            if (message is null)
                return NotFound<string>("Message not found");

            if (message.IsDeleted)
                return Success(message.Id, "Message already deleted");

            if (message.SenderId != caller.Id)
                return Forbidden<string>("You can only delete your own messages");

            var conversation = await _conversations.GetByIdAsync(message.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<string>("Conversation not found");

            if (conversation.IsBot)
                return BadRequest<string>("Bot conversation messages cannot be deleted");

            var activeParticipants = await _participants.GetAllAsync(
                new ConversationParticipantsSpec(conversation.Id, activeOnly: true),
                cancellationToken);

            message.IsDeleted = true;
            message.DeletedOn = DateTime.UtcNow;
            message.Text = null;
            message.ImageUrl = null;
            message.Latitude = null;
            message.Longitude = null;
            _messages.Update(message);
            await _messages.CompleteAsync(cancellationToken);

            await _notifier.NotifyMessageDeletedAsync(
                conversation.Id,
                activeParticipants.Select(p => p.UserId),
                message.Id,
                cancellationToken);

            return Success(message.Id, "Message deleted");
        }
    }
}
