using FitLead.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Trainings
{
    public sealed class Exercise : AggregateRoot<Guid>
    {
        public Guid TrainerId { get; private set; }

        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string? MediaUrl { get; private set; }

        private Exercise() { } // EF

        private Exercise(
            Guid id,
            Guid trainerId,
            string name,
            string description,
            string? mediaUrl)
        {
            Id = id;
            TrainerId = trainerId;
            Name = name;
            Description = description;
            MediaUrl = mediaUrl;
        }

        public static Exercise Create(
            Guid trainerId,
            string name,
            string description,
            string? mediaUrl = null)
        {
            if (trainerId == Guid.Empty)
                throw new ArgumentException("TrainerId is required");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name is required");

            return new Exercise(
                Guid.NewGuid(),
                trainerId,
                name.Trim(),
                description?.Trim() ?? string.Empty,
                mediaUrl);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name is required");

            Name = name.Trim();
        }

        public void UpdateDescription(string description)
        {
            Description = description?.Trim() ?? string.Empty;
        }
    }
}
