using FitLead.Domain.Users;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class UserIdentityLinkConfiguration
        : IEntityTypeConfiguration<UserIdentityLink>
    {
        public void Configure(EntityTypeBuilder<UserIdentityLink> builder)
        {
            builder.ToTable("user_identity_links");

            builder.HasKey(x => x.DomainUserId);

            builder.Property(x => x.DomainUserId)
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(x => x.IdentityUserId)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.IdentityUserId)
                .IsUnique();

            builder.HasOne<User>()
                .WithOne()
                .HasForeignKey<UserIdentityLink>(x => x.DomainUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<AppIdentityUser>()
                .WithOne()
                .HasForeignKey<UserIdentityLink>(x => x.IdentityUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
