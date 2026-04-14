using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Domain.Enums;
using Engzly.DTOs.GigDTOS;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    public sealed class GigsController(ISender _mediator) : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] string? search,
            [FromQuery] string? categoryId,
            [FromQuery] GigStatus? status,
            [FromQuery] decimal? minBudget,
            [FromQuery] decimal? maxBudget,
            [FromQuery] DateTime? startDateFrom,
            [FromQuery] DateTime? dueDateTo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetTasksQuery(
                search, categoryId, status, minBudget, maxBudget, startDateFrom, dueDateTo, page, pageSize));
            return Resolve(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskDetails(string id)
        {
            var result = await _mediator.Send(new GetTaskDetailsQuery(id));
            return Resolve(result);
        }

        [HttpGet("estimate")]
        public async Task<IActionResult> Estimate(
            [FromQuery] string title,
            [FromQuery] string? description,
            [FromQuery] string? categoryId,
            [FromQuery] int numberOfTaskersNeeded = 1)
        {
            var result = await _mediator.Send(
                new EstimateTaskQuery(title, description, categoryId, numberOfTaskersNeeded));
            return Resolve(result);
        }

        [HttpGet("suggest-category")]
        public async Task<IActionResult> SuggestCategory(
            [FromQuery] string title,
            [FromQuery] string? description,
            [FromQuery] int topK = 3)
        {
            var result = await _mediator.Send(new SuggestCategoryQuery(title, description ?? string.Empty, topK));
            return Resolve(result);
        }
        [Authorize]
        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishTaskCommand command)
        {

            var result = await _mediator.Send(command);
            return Resolve(result);
        }

        [Authorize]
        [HttpPut("editegig/{gigid}")]
        public async Task<IActionResult> Edit(string gigid, [FromBody] EditTaskCommand command)
        {
            command.Id = gigid;
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


        [Authorize]
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(string id)
        {
            var result = await _mediator.Send(new CompleteTaskCommand(id));
            return Resolve(result);
        }

        [Authorize]
        [HttpPost("{id}/verify")]
        public async Task<IActionResult> Verify(string id)
        {
            var result = await _mediator.Send(new VerifyTaskCommand(id));
            return Resolve(result);
        }





        [Authorize]
        [HttpPost("{gigId}/apply")]
        public async Task<IActionResult> SubmitProposal([FromRoute] string gigId, [FromBody] SubmitProposalDto massage)
        {

            var _command = new SubmitProposalCommand
            (
                GigId: gigId,
                Message: massage.Message
            );
            var result = await _mediator.Send(_command);
            return Resolve(result);
        }

        [Authorize]
        [HttpGet("{gigId}/proposals")]
        public async Task<IActionResult> GetProposalsForGig([FromRoute] string gigId)
        {
            var result = await _mediator.Send(new GetProposalsByGigQuery(gigId));
            return Resolve(result);
        }

        [Authorize]
        [HttpGet("proposals/mine")]
        public async Task<IActionResult> GetMyProposals()
        {
            var result = await _mediator.Send(new GetMyProposalsQuery());
            return Resolve(result);
        }

        [Authorize]
        [HttpDelete("proposals/{proposalId}")]
        public async Task<IActionResult> WithdrawProposal([FromRoute] string proposalId)
        {
            var result = await _mediator.Send(new WithdrawProposalCommand(proposalId));
            return Resolve(result);
        }

        [Authorize]
        [HttpPatch("requests/{proposalId}")]
        public async Task<IActionResult> DecideOnRequest([FromRoute] string proposalid, [FromBody] DecideOnProposalDto command)
        {

            var _req = new DecideOnProposalCommand
             (
                 ProposalId: proposalid,
                 Decision: (ProposalStatus)command.Decision
             );
            var result = await _mediator.Send((_req));
            return Resolve(result);
        }
    }
}

