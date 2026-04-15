using Engzly.Application.Features.Gigs.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Gigs.Commands.Validators
{
    public sealed class StartGigCommandValidator : AbstractValidator<StartGigCommand>
    {
        public StartGigCommandValidator()
        {
            RuleFor(x => x.GigId).NotEmpty().WithMessage("GigId is required");
        }
    }
}
