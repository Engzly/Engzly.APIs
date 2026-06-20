using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Categories.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.CategoryResponses;
using Engzly.Domain.Entities.Gigs;
using MediatR;

namespace Engzly.Application.Features.Categories.Commands.Handlers
{
    public class UpdateCategoryCommandHandler(IGenericRepository<Category, string> categRepo, ICurrentUserService userService) : ResponseHandler, IRequestHandler<UpdateCategoryCommand, Response<CategoryResponse>>
    {
        public async Task<Response<CategoryResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {

            var user = userService.GetCurrentUser();
            if (user is null || !user.Roles.Contains("Admin"))
                return Unauthorized<CategoryResponse>("You Are Not Have Access To Edit This Category ");

            var category = await categRepo.GetByIdAsync(request.CategoryId);
            if (category is null)
                return NotFound<CategoryResponse>("Category not found.");

            category.Name = request.Name ?? category.Name;
            category.Description = request.Description ?? category.Description;
            categRepo.Update(category);
            var result = await categRepo.CompleteAsync();
            if (result == 0)
                return BadRequest<CategoryResponse>("Failed to update the category.");

            var response = new CategoryResponse
            (
                CategoryId: category.Id,
                Name: category.Name,
                Description: category.Description
            );
            return Success(response, "Category updated successfully.");

        }
    }
}