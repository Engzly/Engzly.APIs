using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.GigConfigurations
{
    public sealed class GigConfiguration : BaseEntityConfigurations<Gig, string>
    {
        public override void Configure(EntityTypeBuilder<Gig> builder)
        {
            base.Configure(builder);

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(EngzlyDbContextSchemeConstants.MaxTitleLength);

            builder.Property(g => g.Description)
                .IsRequired()
                .HasMaxLength(EngzlyDbContextSchemeConstants.MaxDescriptionLength);

            builder.Property(g => g.ImageUrl)
                .HasMaxLength(EngzlyDbContextSchemeConstants.MaxUrlLength);
            
            builder.OwnsOne(g => g.Location, l =>
            {
                l.Property(p => p.Latitude).HasColumnName(nameof(Location.Latitude));
                l.Property(p => p.Longitude).HasColumnName(nameof(Location.Longitude));
            });

            builder.HasOne(g => g.Client)
                .WithMany()
                .HasForeignKey(g => g.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.Category)
                .WithMany()
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
