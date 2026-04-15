using Engzly.Domain.Entities.Payments;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Payments
{
    public sealed class PaymentEventConfiguration : BaseEntityConfigurations<PaymentEvent, string>
    {
        public override void Configure(EntityTypeBuilder<PaymentEvent> builder)
        {
            base.Configure(builder);

            builder.ToTable("PaymentEvents");

            builder.Property(e => e.RawPayload).HasMaxLength(4000);
            builder.HasIndex(e => e.PaymentId);
            builder.HasIndex(e => e.OccurredAtUtc);
        }
    }
}
