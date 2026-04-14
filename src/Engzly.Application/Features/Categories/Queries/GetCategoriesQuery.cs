using Engzly.Application.Common.Bases;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using MediatR;

namespace Engzly.Application.Features.Categories.Queries
{
    public sealed record CategoryItemDto(string Id, string Name, string Description);

    public sealed record GetCategoriesQuery()
        : IRequest<Response<IReadOnlyList<CategoryItemDto>>>;

    public sealed class GetCategoriesQueryHandler(
        IGenericRepository<Category, string> _repo)
        : ResponseHandler, IRequestHandler<GetCategoriesQuery, Response<IReadOnlyList<CategoryItemDto>>>
    {
        public async Task<Response<IReadOnlyList<CategoryItemDto>>> Handle(
            GetCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var categories = await _repo.GetAllAsync(cancellationToken);
            var items = categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryItemDto(c.Id, c.Name, c.Description))
                .ToList();

            return Success<IReadOnlyList<CategoryItemDto>>(items);
        }
    }
}
