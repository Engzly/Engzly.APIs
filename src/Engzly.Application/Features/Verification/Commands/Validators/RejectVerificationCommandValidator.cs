using Engzly.Application.Features.Verification.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Verification.Commands.Validators
{
    public sealed class RejectVerificationCommandValidator : AbstractValidator<RejectVerificationCommand>
    {
        public RejectVerificationCommandValidator()
        {
            RuleFor(x => x.VerificationId).NotEmpty();
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(1000)
                .WithMessage("Rejection reason must be 10-1000 characters");
        }
    }
}
