namespace Engzly.Application.Interfaces.Moderation
{
    public interface IProfanityFilter
    {
        Task<ProfanityResult> CheckAsync(string text, CancellationToken cancellationToken = default);
    }

    public sealed record ProfanityResult(
        bool Flagged,
        double Score,
        IReadOnlyList<string> Categories,
        string? Reason);
}
