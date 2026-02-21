using FitLead.Domain.Users;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class TrainerClientConfiguration
    : IEntityTypeConfiguration<TrainerClient>
    {
        public void Configure(EntityTypeBuilder<TrainerClient> builder)
        {
            builder.ToTable("trainer_clients");

            builder.HasKey(x => new { x.TrainerId, x.ClientId });

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.TrainerId);
            builder.HasIndex(x => x.ClientId);
        }
    }
}
