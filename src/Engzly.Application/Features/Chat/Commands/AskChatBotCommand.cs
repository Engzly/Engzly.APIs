using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces.AI;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Chat.Commands
{
    public sealed record AskChatBotCommand(string Text)
        : IRequest<Response<ChatBotReplyResponse>>, IModeratedTextCommand;

    public sealed class AskChatBotCommandHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ChatMessage, string> _messages,
        ICurrentUserService _currentUser,
        IChatBot _bot)
        : ResponseHandler, IRequestHandler<AskChatBotCommand, Response<ChatBotReplyResponse>>
    {
        private const string BotUserId = "system-bot";
        private const int HistoryLimit = 10;

        public async Task<Response<ChatBotReplyResponse>> Handle(
            AskChatBotCommand request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<ChatBotReplyResponse>();

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest<ChatBotReplyResponse>("text is required");

            var existing = await _conversations.GetAllAsync(
                new BotConversationSpec(caller.Id),
                cancellationToken);

            var conversation = existing.FirstOrDefault();
            if (conversation is null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerId = caller.Id,
                    GigId = null,
                    IsBot = true,
                    CreatedOn = DateTime.UtcNow,
                    LastMessageOn = DateTime.UtcNow
                };
                await _conversations.AddAsync(conversation, cancellationToken);
                await _conversations.CompleteAsync(cancellationToken);
            }

            var priorMessages = await _messages.GetAllAsync(
                new ConversationMessagesSpec(conversation.Id),
                cancellationToken);

            var history = priorMessages
                .Where(m => m.Type == ChatMessageType.Text && !string.IsNullOrWhiteSpace(m.Text))
                .OrderByDescending(m => m.SentOn)
                .Take(HistoryLimit)
                .OrderBy(m => m.SentOn)
                .Select(m => new ChatBotHistoryItem(
                    m.SenderId == BotUserId ? "assistant" : "user",
                    m.Text!))
                .ToList();

            var now = DateTime.UtcNow;
            var userMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderId = caller.Id,
                Type = ChatMessageType.Text,
                Text = request.Text,
                SentOn = now,
                IsRead = true
            };
            await _messages.AddAsync(userMessage, cancellationToken);

            ChatBotReply reply;
            try
            {
                reply = await _bot.AskAsync(request.Text, history, cancellationToken);
            }
            catch
            {
                reply = new ChatBotReply(
                    "Sorry, the assistant is unavailable right now. Please try again later.",
                    "fallback",
                    0.0,
                    "local-fallback");
            }

            var botMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderId = BotUserId,
                Type = ChatMessageType.Text,
                Text = reply.Reply,
                SentOn = DateTime.UtcNow,
                IsRead = false
            };
            await _messages.AddAsync(botMessage, cancellationToken);

            conversation.LastMessageOn = botMessage.SentOn;
            _conversations.Update(conversation);
            await _messages.CompleteAsync(cancellationToken);

            var response = new ChatBotReplyResponse(
                conversation.Id,
                new ChatMessageResponse(
                    userMessage.Id,
                    userMessage.ConversationId,
                    userMessage.SenderId,
                    userMessage.Type,
                    userMessage.Text,
                    null, null, null,
                    userMessage.SentOn,
                    userMessage.IsRead,
                    userMessage.EditedOn,
                    userMessage.IsDeleted),
                new ChatMessageResponse(
                    botMessage.Id,
                    botMessage.ConversationId,
                    botMessage.SenderId,
                    botMessage.Type,
                    botMessage.Text,
                    null, null, null,
                    botMessage.SentOn,
                    botMessage.IsRead,
                    botMessage.EditedOn,
                    botMessage.IsDeleted),
                reply.Intent,
                reply.Confidence);

            return Success(response);
        }

        private sealed class BotConversationSpec : BaseSpecification<Conversation>
        {
            public BotConversationSpec(string userId)
                : base(c => c.IsBot && c.OwnerId == userId)
            { }
        }

        private sealed class ConversationMessagesSpec : BaseSpecification<ChatMessage>
        {
            public ConversationMessagesSpec(string conversationId)
                : base(m => m.ConversationId == conversationId)
            { }
        }
    }
}
