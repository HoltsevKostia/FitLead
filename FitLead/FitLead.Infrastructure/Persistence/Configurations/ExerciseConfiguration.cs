using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class ExerciseConfiguration
    : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ToTable("exercises");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.MediaUrl)
                .HasConversion(
                    mediaUrl => mediaUrl == null ? null : mediaUrl.Value,
                    value => value == null ? null : MediaUrl.Create(value).Value)
                .HasMaxLength(MediaUrl.MaxLength);

            builder.HasIndex(x => x.TrainerId);
        }
    }
}
