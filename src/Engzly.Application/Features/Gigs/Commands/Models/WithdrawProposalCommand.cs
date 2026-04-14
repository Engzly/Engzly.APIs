using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed record WithdrawProposalCommand(string ProposalId) : IRequest<Response<string>>;
}
