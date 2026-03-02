using System.Security.AccessControl;
using MediatR;

namespace Engzly.Application.Features.Reviews.Commands.PostReview;

public sealed record PostReviewCommand(
        string ReviewedUserId,
        string GigId,
        string? Comment,
        float Rating
    ) : IRequest<string>;