using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engzly.Application.Interfaces.AI;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using Microsoft.Extensions.Logging;

namespace Engzly.Infrastructure.AI
{
    public sealed class HttpTaskEstimator(
        HttpClient _http,
        IGenericRepository<Gig, string> _gigRepo,
        IGenericRepository<Category, string> _categoryRepo,
        ICategoryClassifier _classifier,
        ILogger<HttpTaskEstimator> _logger)
        : ITaskEstimator
    {
        private const int MaxExemplars = 50;
        private const string Currency = "EGP";

        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public async Task<TaskEstimate> EstimateAsync(
            TaskEstimationInput input,
            CancellationToken cancellationToken = default)
        {
            var categoryId = input.CategoryId;

            if (string.IsNullOrWhiteSpace(categoryId))
            {
                var suggestions = await _classifier.ClassifyAsync(
                    input.Title,
                    input.Description ?? string.Empty,
                    topK: 1,
                    cancellationToken);
                categoryId = suggestions.FirstOrDefault()?.CategoryId;
            }

            string? categoryName = null;
            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                var category = await _categoryRepo.GetByIdAsync(categoryId, cancellationToken);
                categoryName = category?.Name;
            }

            var exemplars = await LoadExemplarsAsync(categoryId, cancellationToken);

            var payload = new EstimateRequestDto(
                Title: input.Title,
                Description: input.Description ?? string.Empty,
                NumberOfTaskersNeeded: Math.Max(1, input.NumberOfTaskersNeeded),
                TopK: 5,
                Exemplars: exemplars,
                CategoryId: categoryId,
                CategoryName: categoryName,
                Currency: Currency);

            try
            {
                using var response = await _http.PostAsJsonAsync("/estimate", payload, _json, cancellationToken);
                response.EnsureSuccessStatusCode();

                var dto = await response.Content.ReadFromJsonAsync<EstimateResponseDto>(_json, cancellationToken);
                if (dto is null)
                    throw new InvalidOperationException("estimator returned empty payload");

                return Map(dto);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                _logger.LogError(ex, "Task estimator call failed; returning heuristic fallback");
                return BuildFallback(categoryId, categoryName, input);
            }
        }

        private async Task<List<ExemplarDto>> LoadExemplarsAsync(
            string? categoryId,
            CancellationToken ct)
        {
            try
            {
                var spec = new CompletedGigsSpec(categoryId);
                var gigs = await _gigRepo.GetAllAsync(spec, ct);

                return gigs
                    .Where(g => g.Budget > 0 && g.DueDate > g.StartDate)
                    .OrderByDescending(g => g.CreatedOn)
                    .Take(MaxExemplars)
                    .Select(g => new ExemplarDto(
                        Title: g.Title,
                        Description: g.Description ?? string.Empty,
                        Budget: (double)g.Budget,
                        DurationMinutes: (int)(g.DueDate - g.StartDate).TotalMinutes))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load exemplar gigs for estimation");
                return new List<ExemplarDto>();
            }
        }

        private static TaskEstimate Map(EstimateResponseDto dto) => new(
            CategoryId: dto.CategoryId ?? string.Empty,
            CategoryName: dto.CategoryName,
            Budget: new BudgetEstimate(
                Min: (decimal)dto.Budget.Min,
                Expected: (decimal)dto.Budget.Expected,
                Max: (decimal)dto.Budget.Max,
                Currency: dto.Budget.Currency),
            Duration: new DurationEstimate(
                MinMinutes: dto.Duration.MinMinutes,
                ExpectedMinutes: dto.Duration.ExpectedMinutes,
                MaxMinutes: dto.Duration.MaxMinutes),
            Method: dto.Method,
            SampleSize: dto.SampleSize,
            Confidence: dto.Confidence);

        private static TaskEstimate BuildFallback(
            string? categoryId,
            string? categoryName,
            TaskEstimationInput input)
        {
            const decimal baseBudget = 250m;
            const int baseDuration = 120;
            var taskers = Math.Max(1, input.NumberOfTaskersNeeded);
            var factor = ComplexityFactor(input.Description);
            var budgetMult = (decimal)factor * taskers;

            return new TaskEstimate(
                CategoryId: categoryId ?? string.Empty,
                CategoryName: categoryName,
                Budget: new BudgetEstimate(
                    Min: Round(baseBudget * 0.6m * budgetMult),
                    Expected: Round(baseBudget * budgetMult),
                    Max: Round(baseBudget * 1.8m * budgetMult),
                    Currency: Currency),
                Duration: new DurationEstimate(
                    MinMinutes: (int)Math.Round(baseDuration * 0.5 * factor),
                    ExpectedMinutes: (int)Math.Round(baseDuration * factor),
                    MaxMinutes: (int)Math.Round(baseDuration * 2.0 * factor)),
                Method: "fallback-local",
                SampleSize: 0,
                Confidence: 0.2);
        }

        private static double ComplexityFactor(string? description)
        {
            if (string.IsNullOrWhiteSpace(description)) return 1.0;
            var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (words < 20) return 1.0;
            if (words < 60) return 1.15;
            if (words < 120) return 1.30;
            return 1.50;
        }

        private static decimal Round(decimal value)
            => Math.Round(value / 5m, MidpointRounding.AwayFromZero) * 5m;

        private sealed class CompletedGigsSpec : BaseSpecification<Gig>
        {
            public CompletedGigsSpec(string? categoryId)
                : base(g =>
                    (g.Status == GigStatus.Completed || g.Status == GigStatus.PendingVerification) &&
                    (categoryId == null || g.CategoryId == categoryId))
            { }
        }

        private sealed record ExemplarDto(
            [property: JsonPropertyName("title")] string Title,
            [property: JsonPropertyName("description")] string Description,
            [property: JsonPropertyName("budget")] double Budget,
            [property: JsonPropertyName("duration_minutes")] int DurationMinutes);

        private sealed record EstimateRequestDto(
            [property: JsonPropertyName("title")] string Title,
            [property: JsonPropertyName("description")] string Description,
            [property: JsonPropertyName("number_of_taskers_needed")] int NumberOfTaskersNeeded,
            [property: JsonPropertyName("top_k")] int TopK,
            [property: JsonPropertyName("exemplars")] List<ExemplarDto> Exemplars,
            [property: JsonPropertyName("category_id")] string? CategoryId,
            [property: JsonPropertyName("category_name")] string? CategoryName,
            [property: JsonPropertyName("currency")] string Currency);

        private sealed record EstimateResponseDto(
            [property: JsonPropertyName("category_id")] string? CategoryId,
            [property: JsonPropertyName("category_name")] string? CategoryName,
            [property: JsonPropertyName("budget")] BudgetDto Budget,
            [property: JsonPropertyName("duration")] DurationDto Duration,
            [property: JsonPropertyName("method")] string Method,
            [property: JsonPropertyName("sample_size")] int SampleSize,
            [property: JsonPropertyName("confidence")] double Confidence,
            [property: JsonPropertyName("neighbors_used")] int NeighborsUsed);

        private sealed record BudgetDto(
            [property: JsonPropertyName("min")] double Min,
            [property: JsonPropertyName("expected")] double Expected,
            [property: JsonPropertyName("max")] double Max,
            [property: JsonPropertyName("currency")] string Currency);

        private sealed record DurationDto(
            [property: JsonPropertyName("min_minutes")] int MinMinutes,
            [property: JsonPropertyName("expected_minutes")] int ExpectedMinutes,
            [property: JsonPropertyName("max_minutes")] int MaxMinutes);
    }
}
