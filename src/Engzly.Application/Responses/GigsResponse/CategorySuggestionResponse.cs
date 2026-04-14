namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class CategorySuggestionResponse
    {
        public string CategoryId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double Score { get; set; }
    }
}
