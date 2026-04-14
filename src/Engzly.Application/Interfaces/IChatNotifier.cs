using Engzly.Application.Features.Chat.Responses;

namespace Engzly.Application.Interfaces
{
    public interface IChatNotifier
    {
        Task NotifyMessageAsync(string conversationId, string recipientId, ChatMessageResponse message, CancellationToken cancellationToken = default);

        Task NotifyMessageEditedAsync(string conversationId, string recipientId, ChatMessageResponse message, CancellationToken cancellationToken = default);

        Task NotifyMessageDeletedAsync(string conversationId, string recipientId, string messageId, CancellationToken cancellationToken = default);
    }
}
