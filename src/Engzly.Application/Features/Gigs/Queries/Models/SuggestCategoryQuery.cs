using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record SuggestCategoryQuery(string Title, string Description, int TopK = 3)
        : IRequest<Response<IReadOnlyList<CategorySuggestionResponse>>>;
}
