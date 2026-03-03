using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Reviews;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Reviews.Commands.PostReview;

public sealed class PostReviewCommandHandler(
        IGenericRepository<Review,string> reviewRepository,
        IGenericRepository<Gig, string> gigRepository,
        ICurrentUserService currentUserService
        )
    : IRequestHandler<PostReviewCommand, string>
{
    public async Task<string> Handle(PostReviewCommand request, CancellationToken cancellationToken)
    {   
        // 1. Get the current LoggedIn User.
        //var currentUser =  currentUserService.GetCurrentUser();
        
        // 2. Fetch the Gig from the database.
        var spec = new GigForReviewEligibilitySpecification(
                request.GigId,
                "user-123", //currentUser.Id,
                request.ReviewedUserId
            );
        
        // 3. Check Eligibility of the Review using a Specification.
        // 3.1. Reviewer must have done at least one task with the Reviewed User
        // 3.2. The Task [Status] Must be Completed.
        
        var gig = await gigRepository.GetByIdAsync(request.GigId, spec, cancellationToken);
        
        if (gig is null) throw new Exception("Gig Not Found.");

        // 4. Initiate the Review Entity.
        var review = new Review()
        {
            ReviewerId = "user-123",
            ReviewedUserId = request.ReviewedUserId,
            GigId = request.GigId,
            Comment = request.Comment,
            Rating = request.Rating
        };

        // 5. Save the Review to the Database
        await reviewRepository.AddAsync(review, cancellationToken);

        var _ = await reviewRepository.CompleteAsync(cancellationToken);
        
        return review.Id;
    }
}