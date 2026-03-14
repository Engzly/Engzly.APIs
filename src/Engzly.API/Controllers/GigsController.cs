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
        [Authorize]
        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishTaskCommand command)
        {

            var result = await _mediator.Send(command);
            return Resolve(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] EditTaskCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Resolve(result);
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mediator.Send(new DeleteTaskCommand(id));
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

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(string id)
        {
            var result = await _mediator.Send(new CompleteTaskCommand(id));
            return Resolve(result);
        }

        [HttpPost("{id}/verify")]
        public async Task<IActionResult> Verify(string id)
        {
            var result = await _mediator.Send(new VerifyTaskCommand(id));
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

