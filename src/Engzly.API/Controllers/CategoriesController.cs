using Engzly.Application.Features.Categories.Commands.Models;
using Engzly.Application.Features.Categories.Queries;
using Engzly.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers;

public sealed class CategoriesController(ISender mediator) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new GetCategoriesQuery());
        return Resolve(result);
    }

    [Authorize]
    [HttpPost("addCat")]

    public async Task<IActionResult> AddCategory([FromBody] AddCategoryCommand command)
    {
        var result = await mediator.Send(command);
        return Resolve(result);
    }

    [Authorize]
    [HttpPost("updateCat/{categoryId}")]
    public async Task<IActionResult> UpdateCategory(string categoryId, [FromBody] UpdateCategoryDTO command)
    {

        var result = await mediator.Send(new UpdateCategoryCommand
        (
            CategoryId: categoryId,
            Name: command.Name,
            Description: command.Description
        ));
        return Resolve(result);
    }
}
