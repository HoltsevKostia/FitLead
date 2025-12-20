using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure.Persistence.Models
{
    public sealed class TrainerClient
    {
        public Guid TrainerId { get; private set; }
        public Guid ClientId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private TrainerClient() { }

        public TrainerClient(Guid trainerId, Guid clientId)
        {
            TrainerId = trainerId;
            ClientId = clientId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
