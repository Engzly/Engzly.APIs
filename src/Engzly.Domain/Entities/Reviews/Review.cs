using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Domain.Entities.Reviews;

public sealed class Review : BaseEntity<string>
{
    public string GigId { get; set; } = null!;
    public string ReviewerId { get; set; } = null!;
    public string ReviewedUserId { get; set; } = null!;
    public string? Comment { get; set; }
    public float Rating { get; set; }

    public Gig Gig { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
    public User ReviewedUser { get; set; } = null!;
}