using Engzly.Application.Features.Verification.Commands.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Verification.Commands.Validators
{
    public sealed class SubmitVerificationCommandValidator : AbstractValidator<SubmitVerificationCommand>
    {
        private const long MaxFileBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic" };

        public SubmitVerificationCommandValidator()
        {
            RuleFor(x => x.FrontImage).Custom(ValidateFile);
            RuleFor(x => x.BackImage).Custom(ValidateFile);
            RuleFor(x => x.Selfie).Custom(ValidateFile);
        }

        private static void ValidateFile(IFormFile? file, FluentValidation.ValidationContext<SubmitVerificationCommand> ctx)
        {
            if (file is null || file.Length == 0)
            {
                ctx.AddFailure(ctx.PropertyPath, "File is required");
                return;
            }

            if (file.Length > MaxFileBytes)
                ctx.AddFailure(ctx.PropertyPath, "File exceeds 5MB limit");

            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                ctx.AddFailure(ctx.PropertyPath, "File must be jpg, jpeg, png, or heic");
        }
    }
}
