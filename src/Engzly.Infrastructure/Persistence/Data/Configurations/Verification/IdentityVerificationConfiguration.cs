using Engzly.Domain.Entities.Verification;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Verification
{
    public sealed class IdentityVerificationConfiguration : BaseEntityConfigurations<IdentityVerification, string>
    {
        public override void Configure(EntityTypeBuilder<IdentityVerification> builder)
        {
            base.Configure(builder);

            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.Property(v => v.UserId).IsRequired();
            builder.Property(v => v.NationalIdFrontPath).IsRequired().HasMaxLength(512);
            builder.Property(v => v.NationalIdBackPath).IsRequired().HasMaxLength(512);
            builder.Property(v => v.SelfiePath).IsRequired().HasMaxLength(512);
            builder.Property(v => v.Status).HasConversion<int>().IsRequired();
            builder.Property(v => v.SubmittedOn).IsRequired();
            builder.Property(v => v.RejectionReason).HasMaxLength(1000);

            builder.HasOne(v => v.User)
                .WithOne(u => u.IdentityVerification!)
                .HasForeignKey<IdentityVerification>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(v => v.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasIndex(v => new { v.UserId, v.Status })
                .HasFilter("[Status] <> 3")
                .IsUnique();

            builder.HasIndex(v => v.Status);
        }
    }
}
