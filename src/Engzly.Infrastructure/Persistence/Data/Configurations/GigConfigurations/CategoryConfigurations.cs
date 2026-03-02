using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.GigConfigurations;

public sealed class CategoryConfiguration : BaseEntityConfigurations<Category, string>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(EngzlyDbContextSchemeConstants.MaxNameLength);

        builder.Property(c => c.Description)
            .HasMaxLength(EngzlyDbContextSchemeConstants.MaxDescriptionLength);
    }
}