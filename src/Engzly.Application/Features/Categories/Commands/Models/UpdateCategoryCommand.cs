using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.CategoryResponses;
using MediatR;

namespace Engzly.Application.Features.Categories.Commands.Models
{
    public sealed record UpdateCategoryCommand
    (
            string CategoryId,
        string? Name,
        string? Description
        ) : IRequest<Response<CategoryResponse>>;
}
