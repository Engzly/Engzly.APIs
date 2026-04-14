namespace Engzly.Application.Interfaces.AI
{
    public sealed class CategoryAIOptions
    {
        public const string SectionName = "CategoryAI";

        public string BaseUrl { get; set; } = "http://localhost:8000";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);
        public double MinConfidence { get; set; } = 0.35;
        public TimeSpan CandidateCacheTtl { get; set; } = TimeSpan.FromMinutes(5);
    }
}
