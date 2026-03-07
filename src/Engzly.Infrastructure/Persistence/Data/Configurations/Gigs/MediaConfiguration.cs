using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Gigs
{
    public sealed class MediaConfiguration
      : BaseEntityConfigurations<Media, Guid>
    {
        public override void Configure(EntityTypeBuilder<Media> builder)
        {
            base.Configure(builder);

            builder.Property(m => m.Url)
                .IsRequired()
                .HasMaxLength(EngzlyDbContextSchemeConstants.MaxUrlLength);

            builder.Property(m => m.IsTemp)
                .IsRequired();

            builder.HasOne(m => m.Gig)
                .WithMany(g => g.Medias)
                .HasForeignKey(m => m.GigId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
