using Engzly.Application.Features.Reviews.Commands.PostReview;
using Engzly.Application.Features.Reviews.Queries;
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

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetForUser([FromRoute] string userId)
    {
        var result = await mediator.Send(new GetUserReviewsQuery(userId));
        return Ok(result);
    }

    [HttpGet("gig/{gigId}")]
    public async Task<IActionResult> GetForGig([FromRoute] string gigId)
    {
        var result = await mediator.Send(new GetGigReviewsQuery(gigId));
        return Ok(result);
    }

    [HttpGet("user/{userId}/rating")]
    public async Task<IActionResult> GetUserRating([FromRoute] string userId)
    {
        var result = await mediator.Send(new GetUserRatingQuery(userId));
        return Ok(result);
    }
}