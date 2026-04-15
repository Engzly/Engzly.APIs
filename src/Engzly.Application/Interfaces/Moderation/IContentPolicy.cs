namespace Engzly.Application.Interfaces.Moderation
{
    public interface IContentPolicy
    {
        ContentPolicyResult Check(string text);
    }

    public sealed record ContentPolicyResult(bool Allowed, string? Reason);
}
