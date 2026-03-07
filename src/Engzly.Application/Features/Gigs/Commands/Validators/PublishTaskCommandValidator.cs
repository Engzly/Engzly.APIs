
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using FluentValidation;

namespace Engzly.Application.Features.Gigs.Commands.Validators
{
public sealed class PublishTaskCommandValidator : AbstractValidator<PublishTaskCommand>
{
    public PublishTaskCommandValidator(IGenericRepository<Category, string> categoryRepo)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(10, 100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.NumberOfTaskersNeeded)
            .InclusiveBetween(1, 50);

        RuleFor(x => x.Budget)
            .GreaterThan(0);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .MustAsync(async (categoryId, ct) =>
                await categoryRepo.ExistsAsync(new CategoryByIdSpec(categoryId), ct))
            .WithMessage("CategoryId does not exist.");

        // Optional: allow null/empty or require valid URL if provided.
        // If you store relative URLs, change Absolute -> RelativeOrAbsolute.
        RuleFor(x => x.ImageUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("ImageUrl must be a valid URL.");

        // Dates
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("StartDate must be now or in the future.");

        RuleFor(x => x.DueDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("DueDate must be after StartDate.");
    }

    private sealed class CategoryByIdSpec : BaseSpecification<Category>
    {
        public CategoryByIdSpec(string id) : base(c => c.Id == id) { }
    }
}
}