using Engzly.Domain.Entities.Common;

namespace Engzly.Domain.Entities.Notifications;

public class DeviceToken : BaseEntity<long>
{
    public Guid UserId { get; set; }

    public string DeviceTokenValue { get; set; } = null!;

    // Android / iOS / Web
    public string Platform { get; set; } = null!; 

    public string? DeviceId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}