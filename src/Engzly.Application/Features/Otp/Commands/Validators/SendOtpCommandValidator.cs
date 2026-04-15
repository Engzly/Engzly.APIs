using Engzly.Application.Features.Otp.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Otp.Commands.Validators
{
    public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
    {
        public SendOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(200);

            RuleFor(x => x.Purpose)
                .IsInEnum().WithMessage("Purpose is invalid");
        }
    }
}
