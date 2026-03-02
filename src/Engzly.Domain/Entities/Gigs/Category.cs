using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Common;

namespace Engzly.Domain.Entities.Gigs
{
    public sealed class Category : BaseEntity<string>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
