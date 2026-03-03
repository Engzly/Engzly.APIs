using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Gigs
{
    public sealed class ProposalConfiguration : BaseEntityConfigurations<Proposal, string>
    {
        public override void Configure(EntityTypeBuilder<Proposal> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Message)
                .IsRequired()
                .HasMaxLength(EngzlyDbContextSchemeConstants.MaxMessageLength);

            builder.HasOne(p => p.Gig)
                .WithMany()
                .HasForeignKey(p => p.GigId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Tasker)
                .WithMany()
                .HasForeignKey(p => p.TaskerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
