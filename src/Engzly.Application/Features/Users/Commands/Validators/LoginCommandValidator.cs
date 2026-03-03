using Engzly.Application.Features.Users.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Users.Commands.Validators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(l => l.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(l => l.Password)
                .NotEmpty();
        }
    }
}
