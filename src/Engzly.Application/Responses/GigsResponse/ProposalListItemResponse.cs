using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class ProposalListItemResponse
    {
        public string PrposalId { get; set; } = null!;
        public string GigId { get; set; } = null!;
        public string? GigTitle { get; set; }
        public string TaskerId { get; set; } = null!;
        public string? TaskerFullName { get; set; }
        public string Message { get; set; } = null!;
        public ProposalStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}
