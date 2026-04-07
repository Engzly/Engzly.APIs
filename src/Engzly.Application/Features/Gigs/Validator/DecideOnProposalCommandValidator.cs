using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Domain.Enums;
using FluentValidation;

namespace Engzly.Application.Features.Gigs.Validator
{
    public class DecideOnProposalCommandValidator : AbstractValidator<DecideOnProposalCommand>
    {
        public DecideOnProposalCommandValidator()
        {
            RuleFor(x => x.ProposalId)
                .NotEmpty().WithMessage("You must Enter the Proposal ID ")
                .NotNull().WithMessage("Proposal Id Mustn't be Null  ");

            RuleFor(x => x.Decision)
                .IsInEnum().WithMessage("You Must Choose Existing Decision Status ")
                .Must(d => d == ProposalStatus.Approved || d == ProposalStatus.Rejected)
                .WithMessage("Decision Must Be (Approved =1 || Rejected=2 ");
        }
    }
}
