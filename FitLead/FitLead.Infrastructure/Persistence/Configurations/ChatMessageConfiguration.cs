using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("chat_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ChatId)
                .IsRequired();

            builder.Property(x => x.SenderId)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Text)
                .HasMaxLength(ChatMessage.MaxTextLength)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasOne<Chat>()
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ChatId, x.CreatedAtUtc })
                .HasDatabaseName("IX_chat_messages_chat_id_created_at_utc");

            builder.HasIndex(x => new { x.ChatId, x.Id })
                .HasDatabaseName("IX_chat_messages_chat_id_id");

            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_chat_messages_text_required_for_text_type",
                    $"\"Type\" <> {(int)ChatMessageType.Text} OR (\"Text\" IS NOT NULL AND length(btrim(\"Text\")) > 0)");
            });
        }
    }
}
