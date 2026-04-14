using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engzly.Application.Interfaces.AI;
using Microsoft.Extensions.Logging;

namespace Engzly.Infrastructure.AI
{
    public sealed class HttpChatBot(
        HttpClient _http,
        ILogger<HttpChatBot> _logger)
        : IChatBot
    {
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public async Task<ChatBotReply> AskAsync(
            string userMessage,
            IReadOnlyList<ChatBotHistoryItem>? history = null,
            CancellationToken cancellationToken = default)
        {
            var payload = new ChatRequestDto(
                Message: userMessage,
                History: history?.Select(h => new ChatHistoryDto(h.Role, h.Content)).ToList()
                         ?? new List<ChatHistoryDto>());

            try
            {
                using var response = await _http.PostAsJsonAsync("/chat", payload, _json, cancellationToken);
                response.EnsureSuccessStatusCode();

                var dto = await response.Content.ReadFromJsonAsync<ChatResponseDto>(_json, cancellationToken);
                if (dto is null)
                    throw new InvalidOperationException("chatbot returned empty payload");

                return new ChatBotReply(
                    Reply: dto.Reply,
                    Intent: dto.Intent,
                    Confidence: dto.Confidence,
                    Method: dto.Method);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                _logger.LogError(ex, "Chatbot service call failed; returning local fallback");
                return new ChatBotReply(
                    Reply: "Sorry, the assistant is unavailable right now. Please try again in a moment.",
                    Intent: "fallback",
                    Confidence: 0.0,
                    Method: "local-fallback");
            }
        }

        private sealed record ChatHistoryDto(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content")] string Content);

        private sealed record ChatRequestDto(
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("history")] List<ChatHistoryDto> History);

        private sealed record ChatResponseDto(
            [property: JsonPropertyName("reply")] string Reply,
            [property: JsonPropertyName("intent")] string Intent,
            [property: JsonPropertyName("confidence")] double Confidence,
            [property: JsonPropertyName("method")] string Method);
    }
}
