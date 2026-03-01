using Engzly.Domain.Entities.Gigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.GigConfigurations
{
    public class GigAssignmentConfiguration : IEntityTypeConfiguration<GigAssignment>
    {
        public void Configure(EntityTypeBuilder<GigAssignment> builder)
        {
            builder.HasKey(ga => new { ga.GigId, ga.TaskerId });

            builder.HasOne(ga => ga.Gig)
                  .WithMany()
                  .HasForeignKey(ga => ga.GigId);

            builder.HasOne(ga => ga.Tasker)
                  .WithMany()
                  .HasForeignKey(ga => ga.TaskerId);
        }

    }

}
