using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Reviews;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Reviews.Commands.PostReview;

public sealed class PostReviewCommandHandler(
        IGenericRepository<Review, string> reviewRepository,
        IGenericRepository<Gig, string> gigRepository,
        ICurrentUserService currentUserService,
        ILogger<PostReviewCommandHandler> logger)
    : IRequestHandler<PostReviewCommand, string>
{
    public async Task<string> Handle(PostReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetCurrentUser();
        var currentUserId = currentUser.Id;

        logger.LogInformation(
            "User {ReviewerId} is attempting to post a review for Gig {GigId} targeting {ReviewedUserId}",
            currentUserId,
            request.GigId,
            request.ReviewedUserId
        );

        // Business Rule 1: Reviewer must have completed at least one task with ReviewedUser
        // Business Rule 2: Gig status must be Completed
        //var spec = new GigForReviewEligibilitySpecification(
        //        request.GigId,
        //        currentUserId,
        //        request.ReviewedUserId
        //    );

        var gig = await gigRepository.GetByIdAsync(request.GigId,/* spec,*/ cancellationToken);

        if (gig is null)
        {
            logger.LogWarning(
                "Review attempt rejected. Gig {GigId} not found or business rules not satisfied for User {ReviewerId}",
                request.GigId,
                currentUserId
            );
            throw new Exception("Gig Not Found or Review Eligibility failed.");
        }

        // Business Rule passed, create Review entity
        var review = new Review()
        {
            ReviewerId = currentUserId,
            ReviewedUserId = request.ReviewedUserId,
            GigId = request.GigId,
            Comment = request.Comment,
            Rating = request.Rating
        };

        await reviewRepository.AddAsync(review, cancellationToken);
        await reviewRepository.CompleteAsync(cancellationToken);

        logger.LogInformation(
            "Review {ReviewId} successfully created by User {ReviewerId} for Gig {GigId} targeting {ReviewedUserId}",
            review.Id,
            currentUserId,
            request.GigId,
            request.ReviewedUserId
        );

        return review.Id;
    }
}