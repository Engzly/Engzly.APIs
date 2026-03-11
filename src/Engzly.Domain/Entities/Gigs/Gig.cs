using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Gigs
{
    public sealed class Gig : BaseEntity<string>
    {
        public string OwnerId { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Location Location { get; set; }
        public ICollection<Media> Medias { get; set; } = new HashSet<Media>();
        public GigStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public int NumberOfTaskersNeeded { get; set; }
        public DateTime CompletedOn { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Budget { get; set; }

        public User Client { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<GigAssignment> TaskersAssignments { get; set; } = null!;
    }
}
