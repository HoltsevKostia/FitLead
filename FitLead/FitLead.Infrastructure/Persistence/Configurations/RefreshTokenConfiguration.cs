using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.IdentityUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.TokenHash)             
                .IsRequired();

            builder.Property(x => x.FamilyId)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            builder.Property(x => x.RevokedAtUtc);
            builder.Property(x => x.ReplacedByTokenId);
            builder.Property(x => x.ReasonRevoked);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.HasIndex(x => x.IdentityUserId);
            builder.HasIndex(x => x.FamilyId);

            builder.HasOne<AppIdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.IdentityUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<RefreshToken>()
                .WithMany()
                .HasForeignKey(x => x.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
