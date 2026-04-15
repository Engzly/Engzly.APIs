using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record SendImageMessageCommand(string ConversationId, IFormFile Image)
        : IRequest<Response<ChatMessageResponse>>;

    public sealed class SendImageMessageCommandHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ConversationParticipant, string> _participants,
        IGenericRepository<ChatMessage, string> _messages,
        ICurrentUserService _currentUser,
        IFileService _files,
        IChatNotifier _notifier)
        : ResponseHandler, IRequestHandler<SendImageMessageCommand, Response<ChatMessageResponse>>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 5 * 1024 * 1024;

        public async Task<Response<ChatMessageResponse>> Handle(
            SendImageMessageCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<ChatMessageResponse>();

            if (request.Image is null || request.Image.Length == 0)
                return BadRequest<ChatMessageResponse>("image is required");

            var ext = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest<ChatMessageResponse>("Only JPG, JPEG, PNG, WEBP are allowed");

            if (request.Image.Length > MaxImageSize)
                return BadRequest<ChatMessageResponse>("Max image size is 5MB");

            var conversation = await _conversations.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<ChatMessageResponse>("Conversation not found");

            if (conversation.IsBot)
                return BadRequest<ChatMessageResponse>("Images cannot be sent to the chatbot");

            var activeParticipants = await _participants.GetAllAsync(
                new ConversationParticipantsSpec(conversation.Id, activeOnly: true),
                cancellationToken);

            if (!activeParticipants.Any(p => p.UserId == caller.Id))
                return Forbidden<ChatMessageResponse>("You are not an active participant in this conversation");

            var imageUrl = await _files.UploadFileAsync(request.Image, "chat");

            var now = DateTime.UtcNow;
            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderId = caller.Id,
                Type = ChatMessageType.Image,
                ImageUrl = imageUrl,
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
                message.IsRead,
                message.EditedOn,
                message.IsDeleted);

            await _notifier.NotifyMessageAsync(
                conversation.Id,
                activeParticipants.Select(p => p.UserId),
                response,
                cancellationToken);

            return Created(response);
        }
    }
}
