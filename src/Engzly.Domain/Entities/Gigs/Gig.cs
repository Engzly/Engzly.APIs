using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
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
        public string ImageUrl { get; set; } = null!;
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
        public ICollection<User> Taskers { get; set; } = null!;
    }
}
