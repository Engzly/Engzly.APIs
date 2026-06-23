using System.Text.Json.Serialization;

namespace Engzly.Application.Interfaces;

public interface IChatbotService
{
    Task<ChatbotTextResponse> SendMessageAsync(string message, string sessionId);
    Task<ChatbotVoiceResponse> SendVoiceAsync(byte[] audioBytes, string sessionId, bool speakReply = true);
    Task<ChatbotTopicsResponse> GetTopicsAsync();
    Task ResetSessionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
}

public record ChatbotTextResponse(
    [property: JsonPropertyName("reply")] string Reply,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("session_id")] string SessionId
);

public record ChatbotVoiceResponse(
    [property: JsonPropertyName("transcript")] string Transcript,
    [property: JsonPropertyName("reply")] string Reply,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("audio_base64")] string? AudioBase64
);


public record ChatbotTopicsResponse(
    [property: JsonPropertyName("topics")] List<string> Topics,
    [property: JsonPropertyName("total_entries")] int TotalEntries
);