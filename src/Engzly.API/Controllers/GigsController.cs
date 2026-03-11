using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using MassTransit.Mediator;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
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
      



    }
}
