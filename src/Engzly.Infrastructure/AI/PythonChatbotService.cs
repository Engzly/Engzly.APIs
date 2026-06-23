using System.Net.Http.Json;
using Engzly.Application.Interfaces;

namespace Engzly.Infrastructure.Services;

public class PythonChatbotService : IChatbotService
{
    private readonly HttpClient _http;

    public PythonChatbotService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ChatbotTextResponse> SendMessageAsync(string message, string sessionId)
    {
        var response = await _http.PostAsJsonAsync("/chat", new
        {
            message,
            session_id = sessionId
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatbotTextResponse>();
        return result!;
    }

    public async Task<ChatbotVoiceResponse> SendVoiceAsync(byte[] audioBytes, string sessionId, bool speakReply = true)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(audioBytes), "audio", "voice.wav");
        form.Add(new StringContent(sessionId), "session_id");
        form.Add(new StringContent(speakReply.ToString().ToLower()), "speak_reply");

        var response = await _http.PostAsync("/chat/voice", form);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatbotVoiceResponse>();
        if (speakReply)
            return result with { Reply = string.Empty };
        else
            return result with { AudioBase64 = null };

    }


    public async Task<ChatbotTopicsResponse> GetTopicsAsync()
    {
        var response = await _http.GetAsync("/topics");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChatbotTopicsResponse>())!;
    }

    public async Task ResetSessionAsync(string sessionId)
    {
        var response = await _http.PostAsync($"/chat/{sessionId}/reset", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var response = await _http.DeleteAsync($"/chat/{sessionId}");
        response.EnsureSuccessStatusCode();
    }
}