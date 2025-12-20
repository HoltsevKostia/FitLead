using FitLead.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.TrainerId);
            builder.HasIndex(x => x.ClientId);
        }
    }
}
