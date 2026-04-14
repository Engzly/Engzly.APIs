using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Engzly.API.Hubs
{
    public sealed class SignalRChatNotifier(IHubContext<ChatHub> _hub) : IChatNotifier
    {
        public async Task NotifyMessageAsync(
            string conversationId,
            string recipientId,
            ChatMessageResponse message,
            CancellationToken cancellationToken = default)
        {
            var conversationGroup = ChatHub.ConversationGroup(conversationId);
            var userGroup = ChatHub.UserGroup(recipientId);

            await _hub.Clients.Group(conversationGroup)
                .SendAsync("messageReceived", message, cancellationToken);

            await _hub.Clients.Group(userGroup)
                .SendAsync("conversationUpdated", new
                {
                    conversationId,
                    lastMessageOn = message.SentOn,
                    lastMessageType = message.Type,
                    lastMessageText = message.Text
                }, cancellationToken);
        }
    }
}
