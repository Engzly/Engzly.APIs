using Engzly.Application.Features.Otp.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Otp.Commands.Validators
{
    public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Purpose)
                .IsInEnum().WithMessage("Purpose is invalid");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Matches("^\\d{6}$").WithMessage("Code must be a 6-digit numeric value");
        }
    }
}
