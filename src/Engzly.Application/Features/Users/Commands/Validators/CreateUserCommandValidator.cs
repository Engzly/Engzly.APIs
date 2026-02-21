using Engzly.Application.Features.Users.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Users.Commands.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            ApplyValidationRules();
            ApplyCustomValidationsRule();
        }
        
        public void ApplyValidationRules()
        {
            RuleFor(S => S.FullName)
                .NotEmpty().WithMessage("Name is required")
                .Length(1, 100).WithMessage("Name must be between 1 and 100 characters")
                //.Matches(@"^[a-zA-Z]+$").WithMessage("Name contains invalid characters")
                .NotNull().WithMessage("Name must not be null");

            //RuleFor(S => S.City)
            //    .NotEmpty().WithMessage("{PropertyName} is required")
            //    .Length(1, 100).WithMessage("City must be between 1 and 100 characters")
            //    //.Matches(@"^[a-zA-Z]+$").WithMessage("City  contains invalid characters")
            //    .NotNull().WithMessage("{PropertyValue} must not be null");

            //RuleFor(S => S.UserName)
            //    .NotEmpty().WithMessage("{PropertyName} is required")
            //    .Length(1, 100).WithMessage("User_Name must Exist ")
            //    //.Matches(@"^[a-zA-Z]+$").WithMessage("User_Name  contains invalid characters")
            //    .NotNull().WithMessage("{PropertyValue} must not be null");

            //RuleFor(S => S.Password)
            //    .NotEmpty().WithMessage("{PropertyName} is required")
            //    .Length(1, 100).WithMessage("Password must Exist ")
            //    //.Matches(@"^[a-zA-Z]+$").WithMessage("Password  contains invalid characters")
            //    .NotNull().WithMessage("{PropertyValue} must not be null");

            //RuleFor(S => S.PhoneNumber)
            //    .NotEmpty().WithMessage("{PropertyName} is required")
            //    .Length(1, 100).WithMessage("PhoneNumber  must Exist ")
            //    //.Equal(@"^[0-9]+$").WithMessage("PhoneNumber  contains invalid characters")
            //    .NotNull().WithMessage("{PropertyValue} must not be null");
        }

        public void ApplyCustomValidationsRule()
        {
            //RuleFor(s => s.Name)
            //    .MustAsync(async (Model, Key, CancellationToken) => !await _studentService.IsNameExistExcludeSelf(Key, Model.UserName))
            //    .WithMessage("You Can't Update TO THis Name Because Name  Is  Exist  ");
        }
    }

}
