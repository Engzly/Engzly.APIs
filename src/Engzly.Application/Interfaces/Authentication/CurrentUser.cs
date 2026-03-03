namespace Engzly.Application.Interfaces.Authentication;

public sealed record CurrentUser(
        string Id,
        string Email,
        List<string> Roles
        );