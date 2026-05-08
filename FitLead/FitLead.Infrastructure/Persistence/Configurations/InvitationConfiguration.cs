using FitLead.Domain.Invitations;
using FitLead.Domain.Users;
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

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.TokenHash)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            builder.Property(x => x.AcceptedByClientId)
                .IsRequired(false);

            builder.Property(x => x.AcceptedAtUtc)
                .IsRequired(false);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AcceptedByClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.HasIndex(x => x.TrainerId);

            builder.HasIndex(x => new { x.Status, x.ExpiresAtUtc });
        }
    }
}
