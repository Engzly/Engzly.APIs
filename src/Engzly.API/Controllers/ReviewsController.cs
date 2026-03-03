using Engzly.Application.Features.Reviews.Commands.PostReview;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers;

public sealed class ReviewsController(ISender mediator) : BaseApiController
{
    // [Authorize]
    [HttpPost]
    public async Task<ActionResult<string>> Create([FromBody] PostReviewCommand command)
    {
        var response = await mediator.Send(command);

        return Ok(new { ReviewId = response });
    }
}