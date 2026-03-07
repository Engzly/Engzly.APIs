using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Common;

namespace Engzly.Domain.Entities.Gigs
{
    public sealed class Media : BaseEntity<Guid>
    {
        public string Url { get; set; } = null!;

        public bool IsTemp { get; set; } = true;

        public string? GigId { get; set; }

        public Gig? Gig { get; set; }
    }
}
