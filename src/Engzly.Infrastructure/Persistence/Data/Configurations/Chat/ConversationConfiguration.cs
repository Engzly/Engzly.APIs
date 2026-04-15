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

            builder.Property(c => c.GigId).HasMaxLength(450);
            builder.Property(c => c.OwnerId).HasMaxLength(450);
            builder.Property(c => c.IsBot).HasDefaultValue(false);
            builder.Property(c => c.CreatedOn).IsRequired();
            builder.Property(c => c.LastMessageOn).IsRequired();

            builder.HasMany(c => c.Participants)
                .WithOne(p => p.Conversation)
                .HasForeignKey(p => p.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.GigId);
            builder.HasIndex(c => c.OwnerId);
            builder.HasIndex(c => c.LastMessageOn);
        }
    }
}
