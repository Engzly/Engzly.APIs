using Engzly.Domain.Entities.Reviews;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Reviews;

public sealed class ReviewConfigurations : BaseEntityConfigurations<Review, string>
{
    public override void Configure(EntityTypeBuilder<Review> builder)
    {
        base.Configure(builder);
        
        builder.Property(r => r.ReviewerId)
            .IsRequired();

        builder.Property(r => r.ReviewedUserId)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(EngzlyDbContextSchemeConstants.MaxDescriptionLength);

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasPrecision(3, 2);
        
        builder.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5");

        builder.HasIndex(r => new { r.ReviewerId, r.ReviewedUserId, r.GigId })
            .IsUnique();
        
        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(r => r.ReviewedUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Gig)
            .WithMany()
            .HasForeignKey(r => r.GigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}