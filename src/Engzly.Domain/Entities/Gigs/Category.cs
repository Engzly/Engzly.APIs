using Engzly.Domain.Entities.Common;

namespace Engzly.Domain.Entities.Gigs
{
    public sealed class Category : BaseEntity<string>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
