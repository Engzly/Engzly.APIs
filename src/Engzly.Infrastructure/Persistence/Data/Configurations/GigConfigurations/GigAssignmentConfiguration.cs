using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.GigConfigurations
{
    public sealed class GigAssignmentConfiguration : BaseEntityConfigurations<GigAssignment, string>
    {
        public override void Configure(EntityTypeBuilder<GigAssignment> builder)
        {
            base.Configure(builder);
            
            builder.HasIndex(ga => new { ga.GigId, ga.TaskerId }).IsUnique();

            builder.HasOne(ga => ga.Gig)
                .WithMany()
                .HasForeignKey(ga => ga.GigId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ga => ga.Tasker)
                .WithMany()
                .HasForeignKey(ga => ga.TaskerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
