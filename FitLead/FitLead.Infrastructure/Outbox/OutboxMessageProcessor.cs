using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Common.Time;
using FitLead.Domain.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitLead.Infrastructure.Outbox
{
    public sealed class OutboxMessageProcessor : IOutboxMessageProcessor
    {
        private static readonly TimeSpan[] RetryDelays =
        [
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(10)
        ];

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxMessageProcessor> _logger;
        private readonly OutboxProcessorOptions _options;

        public OutboxMessageProcessor(
            IServiceScopeFactory scopeFactory,
            IOptions<OutboxProcessorOptions> options,
            ILogger<OutboxMessageProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        public async Task ProcessAsync(
            Guid messageId,
            CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var clock = scope.ServiceProvider.GetRequiredService<IClock>();
                var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var message = await repository.GetByIdAsync(messageId, cancellationToken);
                if (message is null || message.Status != OutboxMessageStatus.Pending)
                {
                    return;
                }

                await dispatcher.DispatchAsync(message, cancellationToken);

                var markProcessedResult = message.MarkProcessed(clock.UtcNow);
                if (markProcessedResult.IsFailure)
                {
                    throw new InvalidOperationException(markProcessedResult.Error.Message);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Outbox message {OutboxMessageId} processing failed.",
                    messageId);

                await MarkFailedOrRetryAsync(messageId, exception, cancellationToken);
            }
        }

        private async Task MarkFailedOrRetryAsync(
            Guid messageId,
            Exception exception,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var message = await repository.GetByIdAsync(messageId, cancellationToken);
            if (message is null || message.Status != OutboxMessageStatus.Pending)
            {
                return;
            }

            var maxAttempts = exception is UnknownOutboxMessageTypeException
                ? 1
                : _options.MaxAttempts;

            var retryDelay = exception is UnknownOutboxMessageTypeException
                ? TimeSpan.Zero
                : GetRetryDelay(message.RetryCount);

            var markFailedResult = message.MarkFailedOrRetry(
                clock.UtcNow,
                maxAttempts,
                retryDelay,
                exception.Message);

            if (markFailedResult.IsFailure)
            {
                throw new InvalidOperationException(markFailedResult.Error.Message);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static TimeSpan GetRetryDelay(int retryCount)
        {
            return retryCount < RetryDelays.Length
                ? RetryDelays[retryCount]
                : RetryDelays[^1];
        }
    }
}
