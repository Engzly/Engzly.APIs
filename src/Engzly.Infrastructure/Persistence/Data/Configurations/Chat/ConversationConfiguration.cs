using Engzly.Domain.Entities.Chat;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Chat
{
    public sealed class ConversationConfiguration : BaseEntityConfigurations<Conversation, string>
    {
        public override void Configure(EntityTypeBuilder<Conversation> builder)
        {
            base.Configure(builder);

            builder.ToTable("Conversations");

            builder.Property(c => c.UserAId).IsRequired().HasMaxLength(450);
            builder.Property(c => c.UserBId).IsRequired().HasMaxLength(450);
            builder.Property(c => c.GigId).HasMaxLength(450);
            builder.Property(c => c.CreatedOn).IsRequired();
            builder.Property(c => c.LastMessageOn).IsRequired();

            builder.HasOne(c => c.UserA)
                .WithMany()
                .HasForeignKey(c => c.UserAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.UserB)
                .WithMany()
                .HasForeignKey(c => c.UserBId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.UserAId, c.UserBId, c.GigId });
            builder.HasIndex(c => c.LastMessageOn);
        }
    }
}
