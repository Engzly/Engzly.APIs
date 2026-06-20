namespace Engzly.Application.Interfaces.Authentication;

public sealed record CurrentUser(
        string Id,
        string Email,
         string? AccountType,
        List<string> Roles
        );