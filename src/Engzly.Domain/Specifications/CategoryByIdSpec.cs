using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications
{
     public sealed class CategoryByIdSpec : BaseSpecification<Category>
    {
        public CategoryByIdSpec(string id) : base(c => c.Id == id) { }
    }
}