using Engzly.Domain.Entities.Payments;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Payments
{
    public sealed class PaymentConfiguration : BaseEntityConfigurations<Payment, string>
    {
        public override void Configure(EntityTypeBuilder<Payment> builder)
        {
            base.Configure(builder);

            builder.ToTable("Payments");

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(8);

            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.Property(p => p.PlatformCommission).HasPrecision(18, 2);
            builder.Property(p => p.HelperAmount).HasPrecision(18, 2);

            builder.Property(p => p.ProviderInvoiceId).HasMaxLength(128);
            builder.Property(p => p.ProviderInvoiceKey).HasMaxLength(256);
            builder.Property(p => p.ProviderPaymentUrl).HasMaxLength(1024);

            builder.HasIndex(p => p.GigId);
            builder.HasIndex(p => p.ProviderInvoiceId).IsUnique()
                .HasFilter("[ProviderInvoiceId] IS NOT NULL");

            builder.HasOne(p => p.Gig)
                .WithMany()
                .HasForeignKey(p => p.GigId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.ClientUser)
                .WithMany()
                .HasForeignKey(p => p.ClientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.HelperUser)
                .WithMany()
                .HasForeignKey(p => p.HelperUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Events)
                .WithOne(e => e.Payment)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
