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
                .NotEmpty().WithMessage("Name is required")
                .Length(1, 100).WithMessage("Name must be between 1 and 100 characters")
                .NotNull().WithMessage("Name must not be null");
            RuleFor(S => S.LastName)
                .NotEmpty().WithMessage("Name is required")
                .Length(1, 100).WithMessage("Name must be between 1 and 100 characters")
                .NotNull().WithMessage("Name must not be null");

        }

        public void ApplyCustomValidationsRule()
        {
            RuleFor(x => x.Email)
     .MustAsync(async (model, email, cancellation) =>
     {
         var usersWithSameEmail = _userManager.Users
             .Where(u => u.Email == email)
             .ToList();

         if (!usersWithSameEmail.Any())
             return true;

         if (usersWithSameEmail.Count >= 2)
             return false;

         var sameTypeExists = usersWithSameEmail
             .Any(u => u.AccountType == model.AccountType);

         return !sameTypeExists;
     })
     .WithMessage("You can only create two accounts with the same email (Helper and Tasker)");
        }
    }
}

