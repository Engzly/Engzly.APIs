using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Engzly.API.Hubs
{
    [Authorize]
    public sealed class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrWhiteSpace(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrWhiteSpace(userId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));

            await base.OnDisconnectedAsync(exception);
        }

        public Task JoinConversation(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                return Task.CompletedTask;
            return Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
        }

        public Task LeaveConversation(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                return Task.CompletedTask;
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
        }

        public static string UserGroup(string userId) => $"user:{userId}";
        public static string ConversationGroup(string conversationId) => $"conversation:{conversationId}";
    }
}
