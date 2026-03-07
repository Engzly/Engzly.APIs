using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using MediatR;
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

    [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishTaskCommand command)
        {
            var result = await mediator.Send(command);
            return Resolve(result);
        }

     [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] EditTaskCommand command)
        {
            command.Id = id; 
            var result = await mediator.Send(command);
            return Resolve(result);
        }

        [HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
    var result = await mediator.Send(new DeleteTaskCommand(id));
    return Resolve(result);
}

    }
}
