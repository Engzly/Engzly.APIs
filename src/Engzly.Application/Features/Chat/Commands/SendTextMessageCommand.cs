using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record SendTextMessageCommand(string ConversationId, string Text)
        : IRequest<Response<ChatMessageResponse>>;

    public sealed class SendTextMessageCommandHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ChatMessage, string> _messages,
        ICurrentUserService _currentUser,
        IChatNotifier _notifier)
        : ResponseHandler, IRequestHandler<SendTextMessageCommand, Response<ChatMessageResponse>>
    {
        public async Task<Response<ChatMessageResponse>> Handle(
            SendTextMessageCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<ChatMessageResponse>();

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest<ChatMessageResponse>("text is required");

            if (request.Text.Length > 4000)
                return BadRequest<ChatMessageResponse>("text exceeds 4000 characters");

            var conversation = await _conversations.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<ChatMessageResponse>("Conversation not found");

            if (conversation.UserAId != caller.Id && conversation.UserBId != caller.Id)
                return Forbidden<ChatMessageResponse>("Not a participant in this conversation");

            if (conversation.IsBot)
                return BadRequest<ChatMessageResponse>("Use the chatbot endpoint for bot conversations");

            var now = DateTime.UtcNow;
            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderId = caller.Id,
                Type = ChatMessageType.Text,
                Text = request.Text,
                SentOn = now,
                IsRead = false
            };

            await _messages.AddAsync(message, cancellationToken);

            conversation.LastMessageOn = now;
            _conversations.Update(conversation);

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
                message.IsRead);

            var recipientId = conversation.UserAId == caller.Id ? conversation.UserBId : conversation.UserAId;
            await _notifier.NotifyMessageAsync(conversation.Id, recipientId, response, cancellationToken);

            return Created(response);
        }
    }
}
