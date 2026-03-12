using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.GigsResponse
{
    public record DecideOnProposalResponse(
    string Message,
    string ProposalId,
    ProposalStatus Decision,
    string TaskId,
    string TaskStatus,
    string? TaskerId,
    string? ClientId
);
}
