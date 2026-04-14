using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.AI;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public sealed class SuggestCategoryQueryHandler(
        ICategoryClassifier classifier,
        IGenericRepository<Category, string> categoryRepo,
        ILogger<SuggestCategoryQueryHandler> logger)
        : ResponseHandler,
          IRequestHandler<SuggestCategoryQuery, Response<IReadOnlyList<CategorySuggestionResponse>>>
    {
        public async Task<Response<IReadOnlyList<CategorySuggestionResponse>>> Handle(
            SuggestCategoryQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest<IReadOnlyList<CategorySuggestionResponse>>("Title is required");

            var suggestions = await classifier.ClassifyAsync(
                request.Title,
                request.Description ?? string.Empty,
                request.TopK,
                cancellationToken);

            if (suggestions.Count == 0)
            {
                logger.LogWarning("Classifier returned no suggestions for title={Title}", request.Title);
                return Success<IReadOnlyList<CategorySuggestionResponse>>(Array.Empty<CategorySuggestionResponse>());
            }

            var ids = suggestions.Select(s => s.CategoryId).ToHashSet();
            var categories = await categoryRepo.GetAllAsync(cancellationToken);
            var byId = categories
                .Where(c => ids.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.Name);

            var result = suggestions
                .Where(s => byId.ContainsKey(s.CategoryId))
                .Select(s => new CategorySuggestionResponse
                {
                    CategoryId = s.CategoryId,
                    Name = byId[s.CategoryId],
                    Score = s.Score
                })
                .ToList();

            return Success<IReadOnlyList<CategorySuggestionResponse>>(result);
        }
    }
}
