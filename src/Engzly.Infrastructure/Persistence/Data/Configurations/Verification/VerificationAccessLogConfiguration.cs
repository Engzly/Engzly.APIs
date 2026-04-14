using Engzly.Domain.Entities.Verification;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Verification
{
    public sealed class VerificationAccessLogConfiguration : BaseEntityConfigurations<VerificationAccessLog, Guid>
    {
        public override void Configure(EntityTypeBuilder<VerificationAccessLog> builder)
        {
            base.Configure(builder);

            builder.Property(l => l.VerificationId).IsRequired();
            builder.Property(l => l.AccessedByUserId).IsRequired();
            builder.Property(l => l.AccessedOn).IsRequired();
            builder.Property(l => l.DocumentKind).IsRequired().HasMaxLength(32);

            builder.HasOne(l => l.Verification)
                .WithMany()
                .HasForeignKey(l => l.VerificationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(l => l.VerificationId);
            builder.HasIndex(l => l.AccessedOn);
        }
    }
}
