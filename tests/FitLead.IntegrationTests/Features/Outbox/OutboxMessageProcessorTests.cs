using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Outbox;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class OutboxMessageProcessorTests : IntegrationTestBase
{
    public OutboxMessageProcessorTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ProcessAsync_WhenHandlerSucceeds_ShouldMarkProcessed()
    {
        var message = await AddOutboxMessageAsync(TestOutboxMessageTypes.Success);

        await ProcessAsync(message.Id);

        var processed = await GetOutboxMessageAsync(message.Id);
        processed.Status.Should().Be(OutboxMessageStatus.Processed);
        processed.ProcessedAtUtc.Should().NotBeNull();
        processed.RetryCount.Should().Be(0);
        processed.NextRetryAtUtc.Should().BeNull();
        processed.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WhenHandlerFails_ShouldScheduleRetry()
    {
        var message = await AddOutboxMessageAsync(TestOutboxMessageTypes.Failure);

        await ProcessAsync(message.Id);

        var retried = await GetOutboxMessageAsync(message.Id);
        retried.Status.Should().Be(OutboxMessageStatus.Pending);
        retried.RetryCount.Should().Be(1);
        retried.NextRetryAtUtc.Should().NotBeNull();
        retried.Error.Should().Be("Test outbox handler failure.");
    }

    [Fact]
    public async Task ProcessAsync_WhenTypeIsUnknown_ShouldMarkFailed()
    {
        var message = await AddOutboxMessageAsync("Testing.UnknownType");

        await ProcessAsync(message.Id);

        var failed = await GetOutboxMessageAsync(message.Id);
        failed.Status.Should().Be(OutboxMessageStatus.Failed);
        failed.RetryCount.Should().Be(1);
        failed.NextRetryAtUtc.Should().BeNull();
        failed.Error.Should().Contain("Unknown outbox message type");
    }

    private async Task<OutboxMessage> AddOutboxMessageAsync(string type)
    {
        var message = OutboxMessage.Create(
            type,
            """{"id":"test"}""",
            DateTime.UtcNow).Value;

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await repository.AddAsync(message, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        return message;
    }

    private async Task ProcessAsync(Guid messageId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
        await processor.ProcessAsync(messageId, CancellationToken.None);
    }

    private async Task<OutboxMessage> GetOutboxMessageAsync(Guid messageId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();

        return await repository.GetByIdAsync(messageId, CancellationToken.None)
            ?? throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");
    }
}
