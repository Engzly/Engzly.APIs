using Engzly.Domain.Entities.Payments;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Payments
{
    public sealed class HelperWalletConfiguration : BaseEntityConfigurations<HelperWallet, string>
    {
        public override void Configure(EntityTypeBuilder<HelperWallet> builder)
        {
            base.Configure(builder);

            builder.ToTable("HelperWallets");

            builder.Property(w => w.Currency).IsRequired().HasMaxLength(8);
            builder.Property(w => w.Balance).HasPrecision(18, 2);
            builder.Property(w => w.PendingBalance).HasPrecision(18, 2);

            builder.HasIndex(w => w.UserId).IsUnique();

            builder.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
