namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class AssignmentListItemResponse
    {
        public string Id { get; set; } = null!;
        public string GigId { get; set; } = null!;
        public string? GigTitle { get; set; }
        public string ClientId { get; set; } = null!;
        public string? ClientFullName { get; set; }
        public string TaskerId { get; set; } = null!;
        public string? TaskerFullName { get; set; }
        public DateTime AssignedOn { get; set; }
        public bool IsCompletedByTasker { get; set; }
        public DateTime? CompletedByTaskerOn { get; set; }
    }
}
