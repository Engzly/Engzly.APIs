
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Specifications;
using FluentValidation;
namespace Engzly.Application.Features.Gigs.Validator
{
    public sealed class EditTaskCommandValidator : AbstractValidator<EditTaskCommand>
    {
        public EditTaskCommandValidator(
            IGenericRepository<Gig, string> gigRepo,
            IGenericRepository<Category, string> categoryRepo)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, ct) =>
                    await gigRepo.GetByIdAsync(id, ct) is not null)
                .WithMessage("Task does not exist.");

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

              RuleFor(x => x.MediaUrls)
            .Must(urls => urls.Count <= 10)
            .WithMessage("You can upload at most 10 media files.");

        RuleForEach(x => x.MediaUrls)
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Each media URL must be a valid URL.");

            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1))
                .WithMessage("StartDate must be now or in the future.");

            RuleFor(x => x.DueDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("DueDate must be after StartDate.");
        }
    }
}