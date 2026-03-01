using Engzly.Domain.Entities.Gigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.GigConfigurations
{
    public class GigConfiguration : IEntityTypeConfiguration<Gig>
    {
        public void Configure(EntityTypeBuilder<Gig> builder)
        {
            builder.HasOne(g => g.Client)
                   .WithMany()
                   .HasForeignKey(g => g.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.Category)
               .WithMany()
               .HasForeignKey(g => g.CategoryId);

            builder.OwnsOne(g => g.Location);
        }
    }

}
