using Engzly.Domain.Entities.Gigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.GigConfigurations
{
    public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
    {
        public void Configure(EntityTypeBuilder<Proposal> builder)
        {
            builder.HasOne(p => p.Gig)
                 .WithMany()
                 .HasForeignKey(p => p.GigId);

            builder.HasOne(p => p.Tasker)
                 .WithMany()
                 .HasForeignKey(p => p.TaskerId)
                 .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
