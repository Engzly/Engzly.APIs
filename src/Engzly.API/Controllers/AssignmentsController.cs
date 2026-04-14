using Engzly.Application.Features.Gigs.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    [Authorize]
    public sealed class AssignmentsController(ISender _mediator) : BaseApiController
    {
        [HttpGet("mine")]
        public async Task<IActionResult> Mine([FromQuery] string? role)
        {
            var result = await _mediator.Send(new GetMyAssignmentsQuery(role));
            return Resolve(result);
        }
    }
}
