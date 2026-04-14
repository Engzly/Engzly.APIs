using Engzly.Domain.Entities.Chat;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Chat
{
    public sealed class ChatMessageConfiguration : BaseEntityConfigurations<ChatMessage, string>
    {
        public override void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            base.Configure(builder);

            builder.ToTable("ChatMessages");

            builder.Property(m => m.ConversationId).IsRequired().HasMaxLength(450);
            builder.Property(m => m.SenderId).IsRequired().HasMaxLength(450);
            builder.Property(m => m.Type).IsRequired().HasConversion<int>();
            builder.Property(m => m.Text).HasMaxLength(4000);
            builder.Property(m => m.ImageUrl).HasMaxLength(1024);
            builder.Property(m => m.SentOn).IsRequired();
            builder.Property(m => m.IsDeleted).HasDefaultValue(false);

            builder.HasIndex(m => new { m.ConversationId, m.SentOn });
        }
    }
}
