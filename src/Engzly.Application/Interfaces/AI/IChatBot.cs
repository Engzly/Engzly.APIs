namespace Engzly.Application.Interfaces.AI
{
    public interface IChatBot
    {
        Task<ChatBotReply> AskAsync(
            string userMessage,
            IReadOnlyList<ChatBotHistoryItem>? history = null,
            CancellationToken cancellationToken = default);
    }

    public sealed record ChatBotHistoryItem(string Role, string Content);

    public sealed record ChatBotReply(
        string Reply,
        string Intent,
        double Confidence,
        string Method);
}
