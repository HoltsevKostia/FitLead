using FitLead.Application.Common.Time;


namespace FitLead.Infrastructure.Time
{
    public sealed class SystemClock : IClock
    {
        private readonly TimeProvider _timeProvider;

        public SystemClock(TimeProvider timeProvider)
            => _timeProvider = timeProvider;

        public DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
    }
}
