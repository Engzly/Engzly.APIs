using FluentValidation;
using Engzly.Application.Features.Gigs.Commands.Models;
using System.IO;
using System.Linq;

namespace Engzly.Application.Features.Gigs.Validator
{
    public class UploadMediaValidator : AbstractValidator<UploadMediaCommand>
    {
        public UploadMediaValidator()
        {
            RuleFor(x => x.Images)
                .NotEmpty().WithMessage("At least one image is required.");

            RuleForEach(x => x.Images)
                .Must(file => file.Length <= 5 * 1024 * 1024)
                .WithMessage("File size must be <= 5MB");

            RuleForEach(x => x.Images)
                .Must(file =>
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic" };
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    return allowedExtensions.Contains(extension);
                })
                .WithMessage("Unsupported file format. Allowed: jpg, jpeg, png, heic");
        }
    }
}