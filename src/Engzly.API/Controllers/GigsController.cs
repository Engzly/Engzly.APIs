using System.Security.Claims;
using Engzly.API.RequestsModels.GigRequestsModels;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    public sealed class GigsController(ISender _mediator) : BaseApiController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskDetails(string id)
        {
            var result = await _mediator.Send(new GetTaskDetailsQuery(id));
            return Resolve(result);
        }

        [HttpPost("upload-media")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMedia([FromForm] UploadMediaCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.Data == null || !result.Data.Any())
                return BadRequest("No files uploaded");

            return Created("", result.Data);
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

            var result = await _mediator.Send(command);
            return Resolve(result);
        }

        [Authorize]
        [HttpPatch("requests/{proposalId}")]
        public async Task<IActionResult> DecideOnRequest([FromRoute] string proposalid, [FromBody] DecideOnProposalRequestModel command)
        {

            var _req = new DecideOnProposalCommand
             (
                 ProposalId: proposalid,
                    CurrentUserId: User.FindFirstValue("sub"),
                 Decision: command.Decision
             );
            var result = await _mediator.Send((_req));
            return Resolve(result);
        }
    }
}

