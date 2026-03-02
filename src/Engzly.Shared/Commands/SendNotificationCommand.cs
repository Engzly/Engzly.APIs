namespace Engzly.Shared.Commands;

public sealed record SendNotificationCommand(IEnumerable<string> DeviceTokens, string Title, string Body);