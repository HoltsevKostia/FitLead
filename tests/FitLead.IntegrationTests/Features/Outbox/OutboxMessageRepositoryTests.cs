using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Outbox;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class OutboxMessageRepositoryTests : IntegrationTestBase
{
    public OutboxMessageRepositoryTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyDuePendingMessages()
    {
        var utcNow = DateTime.UtcNow;
        var duePending = CreateMessage("Messenger.DuePending", utcNow.AddMinutes(-3));
        var futurePending = CreateMessage("Messenger.FuturePending", utcNow.AddMinutes(-2));
        futurePending.MarkFailedOrRetry(
            utcNow,
            maxAttempts: 3,
            TimeSpan.FromMinutes(10),
            "retry later");
        var processed = CreateMessage("Messenger.Processed", utcNow.AddMinutes(-1));
        processed.MarkProcessed(utcNow);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await repository.AddAsync(duePending, CancellationToken.None);
        await repository.AddAsync(futurePending, CancellationToken.None);
        await repository.AddAsync(processed, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var pending = await repository.GetPendingAsync(
            utcNow,
            batchSize: 10,
            CancellationToken.None);

        pending.Select(message => message.Id).Should().Contain(duePending.Id);
        pending.Select(message => message.Id).Should().NotContain(futurePending.Id);
        pending.Select(message => message.Id).Should().NotContain(processed.Id);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldRespectBatchSizeAndOrdering()
    {
        var utcNow = DateTime.UtcNow;
        var newest = CreateMessage("Messenger.Newest", utcNow.AddMinutes(-1));
        var oldest = CreateMessage("Messenger.Oldest", utcNow.AddMinutes(-3));
        var middle = CreateMessage("Messenger.Middle", utcNow.AddMinutes(-2));

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await repository.AddAsync(newest, CancellationToken.None);
        await repository.AddAsync(oldest, CancellationToken.None);
        await repository.AddAsync(middle, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var pending = await repository.GetPendingAsync(
            utcNow,
            batchSize: 2,
            CancellationToken.None);

        pending.Select(message => message.Id).Should().Equal(oldest.Id, middle.Id);
    }

    [Fact]
    public async Task GetPendingAsync_WithNonPositiveBatchSize_ShouldReturnEmpty()
    {
        var message = CreateMessage("Messenger.Pending", DateTime.UtcNow);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await repository.AddAsync(message, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var pending = await repository.GetPendingAsync(
            DateTime.UtcNow,
            batchSize: 0,
            CancellationToken.None);

        pending.Should().BeEmpty();
    }

    private static OutboxMessage CreateMessage(
        string type,
        DateTime occurredAtUtc)
    {
        return OutboxMessage.Create(
            type,
            """{"id":"test"}""",
            occurredAtUtc).Value;
    }
}
