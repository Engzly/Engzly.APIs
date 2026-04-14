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
    public sealed record SendLocationMessageCommand(string ConversationId, double Latitude, double Longitude)
        : IRequest<Response<ChatMessageResponse>>;

    public sealed class SendLocationMessageCommandHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ChatMessage, string> _messages,
        ICurrentUserService _currentUser,
        IChatNotifier _notifier)
        : ResponseHandler, IRequestHandler<SendLocationMessageCommand, Response<ChatMessageResponse>>
    {
        public async Task<Response<ChatMessageResponse>> Handle(
            SendLocationMessageCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<ChatMessageResponse>();

            if (request.Latitude < -90 || request.Latitude > 90)
                return BadRequest<ChatMessageResponse>("latitude must be between -90 and 90");

            if (request.Longitude < -180 || request.Longitude > 180)
                return BadRequest<ChatMessageResponse>("longitude must be between -180 and 180");

            var conversation = await _conversations.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<ChatMessageResponse>("Conversation not found");

            if (conversation.UserAId != caller.Id && conversation.UserBId != caller.Id)
                return Forbidden<ChatMessageResponse>("Not a participant in this conversation");

            if (conversation.IsBot)
                return BadRequest<ChatMessageResponse>("Locations cannot be sent to the chatbot");

            var now = DateTime.UtcNow;
            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderId = caller.Id,
                Type = ChatMessageType.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
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
