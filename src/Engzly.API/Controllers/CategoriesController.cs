using Engzly.Application.Features.Categories.Queries;
using MediatR;
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
}
