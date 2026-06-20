using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Categories.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.CategoryResponses;
using Engzly.Domain.Entities.Gigs;
using MediatR;

namespace Engzly.Application.Features.Categories.Commands.Handlers
{
    public class AddCategoryCommandHandler(IGenericRepository<Category, string> categRepo, IMapper mapper, ICurrentUserService userService) : ResponseHandler, IRequestHandler<AddCategoryCommand, Response<CategoryResponse>>
    {
        public async Task<Response<CategoryResponse>> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
        {

            var user = userService.GetCurrentUser();
            if (user is null || !user.Roles.Contains("Admin"))
                return Unauthorized<CategoryResponse>("You Are Not Have Access To Add Category");

            // Map the request to the Category entity
            var category = mapper.Map<Category>(request);
            // Add the new category to the repository
            await categRepo.AddAsync(category);
            // Map the added category to a CategoryResponse
            var result = await categRepo.CompleteAsync();
            if (result == 0)
                return BadRequest<CategoryResponse>("Failed to add the category.");

            var response = new CategoryResponse
            (
                CategoryId: category.Id,
                Name: category.Name,
                Description: category.Description
            );

            return Created(response);
        }
    }
}