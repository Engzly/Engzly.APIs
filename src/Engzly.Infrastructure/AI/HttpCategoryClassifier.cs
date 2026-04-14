using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engzly.Application.Interfaces.AI;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Engzly.Infrastructure.AI
{
    // CategoryAIOptions now lives in Engzly.Application.Interfaces.AI
    public sealed class HttpCategoryClassifier : ICategoryClassifier
    {
        private readonly HttpClient _http;
        private readonly IGenericRepository<Category, string> _categoryRepo;
        private readonly ILogger<HttpCategoryClassifier> _logger;
        private readonly CategoryAIOptions _options;

        private static readonly SemaphoreSlim _cacheLock = new(1, 1);
        private static List<CandidateDto>? _cachedCandidates;
        private static DateTime _cacheLoadedAt;

        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public HttpCategoryClassifier(
            HttpClient http,
            IGenericRepository<Category, string> categoryRepo,
            IOptions<CategoryAIOptions> options,
            ILogger<HttpCategoryClassifier> logger)
        {
            _http = http;
            _categoryRepo = categoryRepo;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CategorySuggestion>> ClassifyAsync(
            string title,
            string description,
            int topK,
            CancellationToken cancellationToken = default)
        {
            var candidates = await GetCandidatesAsync(cancellationToken);
            if (candidates.Count == 0)
            {
                _logger.LogWarning("No categories available in database for classification");
                return Array.Empty<CategorySuggestion>();
            }

            var request = new ClassifyRequestDto(
                Title: title,
                Description: description ?? string.Empty,
                TopK: Math.Clamp(topK, 1, 20),
                Candidates: candidates);

            try
            {
                using var response = await _http.PostAsJsonAsync("/classify", request, _json, cancellationToken);
                response.EnsureSuccessStatusCode();

                var payload = await response.Content.ReadFromJsonAsync<ClassifyResponseDto>(_json, cancellationToken);
                if (payload?.Results is null)
                    return Array.Empty<CategorySuggestion>();

                return payload.Results
                    .Select(r => new CategorySuggestion(r.CategoryId, r.Score))
                    .ToList();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "Category classifier call failed");
                return Array.Empty<CategorySuggestion>();
            }
        }

        private async Task<List<CandidateDto>> GetCandidatesAsync(CancellationToken ct)
        {
            if (_cachedCandidates is not null && DateTime.UtcNow - _cacheLoadedAt < _options.CandidateCacheTtl)
                return _cachedCandidates;

            await _cacheLock.WaitAsync(ct);
            try
            {
                if (_cachedCandidates is not null && DateTime.UtcNow - _cacheLoadedAt < _options.CandidateCacheTtl)
                    return _cachedCandidates;

                var categories = await _categoryRepo.GetAllAsync(ct);
                _cachedCandidates = categories
                    .Select(c => new CandidateDto(c.Id, c.Name, c.Description ?? string.Empty))
                    .ToList();
                _cacheLoadedAt = DateTime.UtcNow;
                return _cachedCandidates;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private sealed record CandidateDto(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("description")] string Description);

        private sealed record ClassifyRequestDto(
            [property: JsonPropertyName("title")] string Title,
            [property: JsonPropertyName("description")] string Description,
            [property: JsonPropertyName("top_k")] int TopK,
            [property: JsonPropertyName("candidates")] List<CandidateDto> Candidates);

        private sealed record ClassifyResponseDto(
            [property: JsonPropertyName("results")] List<SuggestionDto> Results);

        private sealed record SuggestionDto(
            [property: JsonPropertyName("categoryId")] string CategoryId,
            [property: JsonPropertyName("score")] double Score);
    }
}
