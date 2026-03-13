namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class TaskDetailedResponse
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public ClientInfoResponse ClientInfo { get; set; } = null!;
        public List<string> MediaUrls { get; set; } = new();
        public int RequiredHelpers { get; set; }
        public int CurrentFilledCount { get; set; }
        public bool IsAppliedByMe { get; set; }
        public string Status { get; set; } = null!;
    }
}
