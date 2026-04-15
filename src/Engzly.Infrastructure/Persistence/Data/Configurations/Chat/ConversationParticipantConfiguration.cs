using Engzly.Domain.Entities.Chat;
using Engzly.Infrastructure.Persistence.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engzly.Infrastructure.Persistence.Data.Configurations.Chat
{
    public sealed class ConversationParticipantConfiguration : BaseEntityConfigurations<ConversationParticipant, string>
    {
        public override void Configure(EntityTypeBuilder<ConversationParticipant> builder)
        {
            base.Configure(builder);

            builder.ToTable("ConversationParticipants");

            builder.Property(p => p.ConversationId).IsRequired().HasMaxLength(450);
            builder.Property(p => p.UserId).IsRequired().HasMaxLength(450);
            builder.Property(p => p.Role).IsRequired().HasConversion<int>();
            builder.Property(p => p.JoinedOn).IsRequired();
            builder.Property(p => p.LeaveReason).HasMaxLength(500);

            builder.HasIndex(p => new { p.ConversationId, p.UserId });
            builder.HasIndex(p => p.UserId);
        }
    }
}
