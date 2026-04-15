using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Engzly.API.Hubs
{
    public sealed class SignalRChatNotifier(IHubContext<ChatHub> _hub) : IChatNotifier
    {
        public async Task NotifyMessageAsync(
            string conversationId,
            IEnumerable<string> participantUserIds,
            ChatMessageResponse message,
            CancellationToken cancellationToken = default)
        {
            var conversationGroup = ChatHub.ConversationGroup(conversationId);

            await _hub.Clients.Group(conversationGroup)
                .SendAsync("messageReceived", message, cancellationToken);

            var summary = new
            {
                conversationId,
                lastMessageOn = message.SentOn,
                lastMessageType = message.Type,
                lastMessageText = message.Text
            };

            foreach (var userId in participantUserIds.Distinct())
            {
                await _hub.Clients.Group(ChatHub.UserGroup(userId))
                    .SendAsync("conversationUpdated", summary, cancellationToken);
            }
        }

        public async Task NotifyMessageEditedAsync(
            string conversationId,
            IEnumerable<string> participantUserIds,
            ChatMessageResponse message,
            CancellationToken cancellationToken = default)
        {
            var conversationGroup = ChatHub.ConversationGroup(conversationId);

            await _hub.Clients.Group(conversationGroup)
                .SendAsync("messageEdited", message, cancellationToken);

            var payload = new
            {
                conversationId,
                messageId = message.Id,
                text = message.Text,
                editedOn = message.EditedOn
            };

            foreach (var userId in participantUserIds.Distinct())
            {
                await _hub.Clients.Group(ChatHub.UserGroup(userId))
                    .SendAsync("conversationMessageEdited", payload, cancellationToken);
            }
        }

        public async Task NotifyMessageDeletedAsync(
            string conversationId,
            IEnumerable<string> participantUserIds,
            string messageId,
            CancellationToken cancellationToken = default)
        {
            var conversationGroup = ChatHub.ConversationGroup(conversationId);
            var payload = new { conversationId, messageId };

            await _hub.Clients.Group(conversationGroup)
                .SendAsync("messageDeleted", payload, cancellationToken);

            foreach (var userId in participantUserIds.Distinct())
            {
                await _hub.Clients.Group(ChatHub.UserGroup(userId))
                    .SendAsync("conversationMessageDeleted", payload, cancellationToken);
            }
        }

        public async Task NotifyParticipantJoinedAsync(
            string conversationId,
            IEnumerable<string> participantUserIds,
            ConversationParticipantItem participant,
            CancellationToken cancellationToken = default)
        {
            var conversationGroup = ChatHub.ConversationGroup(conversationId);
            var payload = new { conversationId, participant };

            await _hub.Clients.Group(conversationGroup)
                .SendAsync("participantJoined", payload, cancellationToken);

            foreach (var userId in participantUserIds.Distinct())
            {
                await _hub.Clients.Group(ChatHub.UserGroup(userId))
                    .SendAsync("conversationParticipantJoined", payload, cancellationToken);
            }
        }

        public async Task NotifyParticipantLeftAsync(
            string conversationId,
            IEnumerable<string> participantUserIds,
            string userId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var conversationGroup = ChatHub.ConversationGroup(conversationId);
            var payload = new { conversationId, userId, reason };

            await _hub.Clients.Group(conversationGroup)
                .SendAsync("participantLeft", payload, cancellationToken);

            foreach (var uid in participantUserIds.Distinct())
            {
                await _hub.Clients.Group(ChatHub.UserGroup(uid))
                    .SendAsync("conversationParticipantLeft", payload, cancellationToken);
            }
        }
    }
}
