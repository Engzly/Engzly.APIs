using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engzly.Application.Interfaces.Moderation;
using Microsoft.Extensions.Logging;

namespace Engzly.Infrastructure.Moderation
{
    public sealed class HttpProfanityFilter(
        HttpClient _http,
        ILogger<HttpProfanityFilter> _logger)
        : IProfanityFilter
    {
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public async Task<ProfanityResult> CheckAsync(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _http.PostAsJsonAsync(
                    "/moderate",
                    new ModerateRequestDto(text),
                    _json,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var dto = await response.Content.ReadFromJsonAsync<ModerateResponseDto>(_json, cancellationToken);
                if (dto is null)
                    throw new InvalidOperationException("moderation service returned empty payload");

                return new ProfanityResult(
                    Flagged: dto.Flagged,
                    Score: dto.Score,
                    Categories: (IReadOnlyList<string>?)dto.Categories ?? Array.Empty<string>(),
                    Reason: dto.Flagged ? "Message contains inappropriate language" : null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                _logger.LogWarning(ex, "Profanity filter unavailable — failing open");
                return new ProfanityResult(false, 0.0, Array.Empty<string>(), null);
            }
        }

        private sealed record ModerateRequestDto(
            [property: JsonPropertyName("text")] string Text);

        private sealed record ModerateResponseDto(
            [property: JsonPropertyName("flagged")] bool Flagged,
            [property: JsonPropertyName("score")] double Score,
            [property: JsonPropertyName("categories")] List<string>? Categories,
            [property: JsonPropertyName("method")] string? Method);
    }
}
