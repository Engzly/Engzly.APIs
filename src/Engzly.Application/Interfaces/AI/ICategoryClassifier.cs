namespace Engzly.Application.Interfaces.AI
{
    public interface ICategoryClassifier
    {
        Task<IReadOnlyList<CategorySuggestion>> ClassifyAsync(
            string title,
            string description,
            int topK,
            CancellationToken cancellationToken = default);
    }

    public sealed record CategorySuggestion(string CategoryId, double Score);
}
