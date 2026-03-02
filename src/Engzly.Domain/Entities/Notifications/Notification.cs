using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Domain.Entities.Notifications;

public sealed class Notification : BaseEntity<Guid>
{
    public string UserId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public bool IsRead { get; set; }

    public User User { get; set; } = null!;
}