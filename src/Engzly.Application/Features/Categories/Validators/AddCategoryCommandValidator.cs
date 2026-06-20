using Engzly.Application.Features.Categories.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Categories.Validators
{
    public sealed class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
    {
        public AddCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category Name is required.")
                .NotNull().WithMessage("Category Name must not be null.")
                .MaximumLength(100).WithMessage("Category Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
             .NotEmpty().WithMessage("Category Description is required.")
             .NotNull().WithMessage("Category Description must not be null.")
             .MaximumLength(300).WithMessage("Category Description must not exceed 300 characters.");
        }

    }
}
