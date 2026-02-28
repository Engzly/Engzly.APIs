using Engzly.Application.Features.Otp.Commands.Models;
using Engzly.Domain.Enums;
using FluentValidation;

namespace Engzly.Application.Features.Otp.Commands.Validators
{
    public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
    {
        public SendOtpCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Purpose)
                .IsInEnum().WithMessage("Purpose is invalid");

            RuleFor(x => x.Channel)
                .IsInEnum().WithMessage("Channel is invalid");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination is required")
                .MaximumLength(200).WithMessage("Destination is too long");

            // When sending via email, destination must be an email address
            When(x => x.Channel == OtpChannel.Email, () =>
            {
                RuleFor(x => x.Destination)
                    .EmailAddress().WithMessage("Destination must be a valid email address for Email channel");
            });

            // When sending via WhatsApp, validate E.164 phone number
            When(x => x.Channel == OtpChannel.WhatsApp, () =>
            {
                RuleFor(x => x.Destination)
                    .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Destination must be a valid E.164 phone number for WhatsApp channel");
            });
        }
    }
}
