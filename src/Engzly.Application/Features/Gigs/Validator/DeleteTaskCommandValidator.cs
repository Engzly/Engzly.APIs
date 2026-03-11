using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using FluentValidation;

namespace Engzly.Application.Features.Gigs.Commands.Validators
{
    public sealed class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
    {
        public DeleteTaskCommandValidator(IGenericRepository<Gig, string> gigRepo)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .MustAsync(async (id, ct) =>
                    await gigRepo.GetByIdAsync(id, ct) is not null)
                .WithMessage("Task does not exist.");
        }
    }
}
