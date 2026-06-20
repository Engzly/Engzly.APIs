using Engzly.Application.Features.Categories.Commands.Models;
using FluentValidation;

namespace Engzly.Application.Features.Categories.Validators
{
    public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category Id is required.")
                .NotNull().WithMessage("Category Id cannot be null.");
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Category Name cannot exceed 100 characters.");
            RuleFor(x => x.Description)
                .MaximumLength(300).WithMessage("Category Description cannot exceed 300 characters.");
        }
    }
}
