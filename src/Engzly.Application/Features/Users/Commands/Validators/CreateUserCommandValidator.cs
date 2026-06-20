using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        private readonly UserManager<User> _userManager;
        public CreateUserCommandValidator(UserManager<User> userManager)
        {
            _userManager = userManager;
            ApplyValidationRules();
            ApplyCustomValidationsRule();
        }

        public void ApplyValidationRules()
        {
            RuleFor(S => S.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 100).WithMessage("First name must be between 1 and 100 characters");

            RuleFor(S => S.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 100).WithMessage("Last name must be between 1 and 100 characters");

            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        }

        public void ApplyCustomValidationsRule()
        {
            RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) =>
            {
                var userExists = await _userManager.FindByEmailAsync(email);
                return userExists == null;
            })
            .WithMessage("Email already exists. So You Can't Create More than one Account  Please use another email.");

        }
    }
}