using FitLead.Domain.Invitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class InvitationConfiguration
        : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.ToTable("invitations");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            // Status (enum)
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.HasIndex(x => x.ClientId);
            builder.HasIndex(x => x.TrainerId);

            // only one pending invitation between trainer and client
            builder.HasIndex(x => new { x.TrainerId, x.ClientId, x.Status })
                .IsUnique()
                .HasFilter("\"Status\" = 0"); // Pending
        }
    }
}
