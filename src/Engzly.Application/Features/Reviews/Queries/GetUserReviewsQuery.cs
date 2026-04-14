using MediatR;

namespace Engzly.Application.Features.Reviews.Queries
{
    public sealed record ReviewItemDto(
        string Id,
        string GigId,
        string ReviewerId,
        string ReviewedUserId,
        string? Comment,
        decimal Rating);

    public sealed record GetUserReviewsQuery(string UserId)
        : IRequest<IReadOnlyList<ReviewItemDto>>;

    public sealed record GetGigReviewsQuery(string GigId)
        : IRequest<IReadOnlyList<ReviewItemDto>>;

    public sealed record GetUserRatingQuery(string UserId)
        : IRequest<UserRatingDto>;

    public sealed record UserRatingDto(string UserId, decimal AverageRating, int ReviewCount);
}
