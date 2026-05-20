using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Common.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitLead.Infrastructure.Outbox
{
    public sealed class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOutboxMessageProcessor _messageProcessor;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly OutboxProcessorOptions _options;

        public OutboxProcessor(
            IServiceScopeFactory scopeFactory,
            IOutboxMessageProcessor messageProcessor,
            IOptions<OutboxProcessorOptions> options,
            ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _messageProcessor = messageProcessor;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                return;
            }

            var pollingInterval = TimeSpan.FromSeconds(_options.PollingIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Outbox processor loop failed.");
                }

                await Task.Delay(pollingInterval, stoppingToken);
            }
        }

        private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<Guid> messageIds;

            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var clock = scope.ServiceProvider.GetRequiredService<IClock>();
                var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();

                var pendingMessages = await repository.GetPendingAsync(
                    clock.UtcNow,
                    _options.BatchSize,
                    cancellationToken);

                messageIds = pendingMessages
                    .Select(message => message.Id)
                    .ToList();
            }

            foreach (var messageId in messageIds)
            {
                await _messageProcessor.ProcessAsync(messageId, cancellationToken);
            }
        }
    }
}
