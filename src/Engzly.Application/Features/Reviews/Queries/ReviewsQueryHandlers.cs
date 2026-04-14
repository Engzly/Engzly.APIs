using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Reviews;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Reviews.Queries
{
    public sealed class GetUserReviewsQueryHandler(IGenericRepository<Review, string> _repo)
        : IRequestHandler<GetUserReviewsQuery, IReadOnlyList<ReviewItemDto>>
    {
        public async Task<IReadOnlyList<ReviewItemDto>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _repo.GetAllAsync(new ReviewsByReviewedUserSpec(request.UserId), cancellationToken);
            return reviews
                .Select(r => new ReviewItemDto(r.Id, r.GigId, r.ReviewerId, r.ReviewedUserId, r.Comment, r.Rating))
                .ToList();
        }

        private sealed class ReviewsByReviewedUserSpec : BaseSpecification<Review>
        {
            public ReviewsByReviewedUserSpec(string userId) : base(r => r.ReviewedUserId == userId) { }
        }
    }

    public sealed class GetGigReviewsQueryHandler(IGenericRepository<Review, string> _repo)
        : IRequestHandler<GetGigReviewsQuery, IReadOnlyList<ReviewItemDto>>
    {
        public async Task<IReadOnlyList<ReviewItemDto>> Handle(GetGigReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _repo.GetAllAsync(new ReviewsByGigSpec(request.GigId), cancellationToken);
            return reviews
                .Select(r => new ReviewItemDto(r.Id, r.GigId, r.ReviewerId, r.ReviewedUserId, r.Comment, r.Rating))
                .ToList();
        }

        private sealed class ReviewsByGigSpec : BaseSpecification<Review>
        {
            public ReviewsByGigSpec(string gigId) : base(r => r.GigId == gigId) { }
        }
    }

    public sealed class GetUserRatingQueryHandler(IGenericRepository<Review, string> _repo)
        : IRequestHandler<GetUserRatingQuery, UserRatingDto>
    {
        public async Task<UserRatingDto> Handle(GetUserRatingQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _repo.GetAllAsync(new ReviewsByReviewedUserSpec(request.UserId), cancellationToken);
            if (reviews.Count == 0)
                return new UserRatingDto(request.UserId, 0m, 0);

            var avg = reviews.Average(r => r.Rating);
            return new UserRatingDto(request.UserId, Math.Round(avg, 2), reviews.Count);
        }

        private sealed class ReviewsByReviewedUserSpec : BaseSpecification<Review>
        {
            public ReviewsByReviewedUserSpec(string userId) : base(r => r.ReviewedUserId == userId) { }
        }
    }
}
