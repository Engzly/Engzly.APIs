using Engzly.Application.Features.Admin.Users.Commands;
using Engzly.Application.Features.Admin.Users.Queries;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
[ApiController]
public sealed class AdminUsersController(ISender mediator) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] AccountType? accountType,
        [FromQuery] UserStatus? status,
        [FromQuery] bool? isIdentityVerified,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetUsersQuery(
            search, accountType, status, isIdentityVerified, page, pageSize));
        return Resolve(result);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById([FromRoute] string userId)
    {
        var result = await mediator.Send(new GetUserByIdQuery(userId));
        return Resolve(result);
    }

    [HttpPatch("{userId}/block")]
    public async Task<IActionResult> Block([FromRoute] string userId)
    {
        var result = await mediator.Send(new BlockUserCommand(userId));
        return Resolve(result);
    }

    [HttpPatch("{userId}/unblock")]
    public async Task<IActionResult> Unblock([FromRoute] string userId)
    {
        var result = await mediator.Send(new UnblockUserCommand(userId));
        return Resolve(result);
    }
}
