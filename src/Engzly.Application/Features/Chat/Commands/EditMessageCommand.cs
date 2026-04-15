using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record EditMessageCommand(string MessageId, string Text)
        : IRequest<Response<ChatMessageResponse>>, IModeratedTextCommand;

    public sealed class EditMessageCommandHandler(
        IGenericRepository<ChatMessage, string> _messages,
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ConversationParticipant, string> _participants,
        ICurrentUserService _currentUser,
        IChatNotifier _notifier)
        : ResponseHandler, IRequestHandler<EditMessageCommand, Response<ChatMessageResponse>>
    {
        public async Task<Response<ChatMessageResponse>> Handle(
            EditMessageCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<ChatMessageResponse>();

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest<ChatMessageResponse>("text is required");

            if (request.Text.Length > 4000)
                return BadRequest<ChatMessageResponse>("text exceeds 4000 characters");

            var message = await _messages.GetByIdAsync(request.MessageId, cancellationToken);
            if (message is null)
                return NotFound<ChatMessageResponse>("Message not found");

            if (message.IsDeleted)
                return BadRequest<ChatMessageResponse>("Cannot edit a deleted message");

            if (message.SenderId != caller.Id)
                return Forbidden<ChatMessageResponse>("You can only edit your own messages");

            if (message.Type != ChatMessageType.Text)
                return BadRequest<ChatMessageResponse>("Only text messages can be edited");

            var conversation = await _conversations.GetByIdAsync(message.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<ChatMessageResponse>("Conversation not found");

            if (conversation.IsBot)
                return BadRequest<ChatMessageResponse>("Bot conversation messages cannot be edited");

            var activeParticipants = await _participants.GetAllAsync(
                new ConversationParticipantsSpec(conversation.Id, activeOnly: true),
                cancellationToken);

            if (!activeParticipants.Any(p => p.UserId == caller.Id))
                return Forbidden<ChatMessageResponse>("You are not an active participant in this conversation");

            message.Text = request.Text;
            message.EditedOn = DateTime.UtcNow;
            _messages.Update(message);
            await _messages.CompleteAsync(cancellationToken);

            var response = new ChatMessageResponse(
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.Type,
                message.Text,
                message.ImageUrl,
                message.Latitude,
                message.Longitude,
                message.SentOn,
                message.IsRead,
                message.EditedOn,
                message.IsDeleted);

            await _notifier.NotifyMessageEditedAsync(
                conversation.Id,
                activeParticipants.Select(p => p.UserId),
                response,
                cancellationToken);

            return Success(response, "Message edited");
        }
    }
}
