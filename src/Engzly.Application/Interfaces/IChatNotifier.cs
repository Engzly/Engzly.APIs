using Engzly.Application.Features.Chat.Responses;

namespace Engzly.Application.Interfaces
{
    public interface IChatNotifier
    {
        Task NotifyMessageAsync(string conversationId, string recipientId, ChatMessageResponse message, CancellationToken cancellationToken = default);
    }
}
