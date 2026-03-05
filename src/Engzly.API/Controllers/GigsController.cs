using System.Security.Claims;
using Engzly.API.RequestsModels.GigRequestsModels;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    public sealed class GigsController(ISender mediator) : BaseApiController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskDetails(string id)
        {
            var result = await mediator.Send(new GetTaskDetailsQuery(id));
            return Resolve(result);
        }

        [Authorize]
        [HttpPost("{gigId}/apply")]
        public async Task<IActionResult> SubmitProposal([FromRoute] string gigId, [FromBody] SubmitProposalRequestModel request)
        {

            var command = new SubmitProposalCommand
            (
                GigId: gigId,
                CurrentUserId: User.FindFirstValue("sub"),
                Message: request.Message
            );

            var result = await mediator.Send(command);
            return Resolve(result);
        }
    }
}
