using Engzly.Domain.Enums;

namespace Engzly.API.RequestsModels.GigRequestsModels
{

    public record DecideOnProposalRequestModel
        (
            ProposalStatus Decision
        );

}
