using FitLead.Domain.Outbox;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Outbox;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Create_WithValidPayload_ShouldCreatePendingMessage()
    {
        var occurredAtUtc = DateTime.UtcNow;

        var result = OutboxMessage.Create(
            "  Messenger.ChatMessageCreated  ",
            """{"messageId":"message-id"}""",
            occurredAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Type.Should().Be("Messenger.ChatMessageCreated");
        result.Value.Payload.Should().Be("""{"messageId":"message-id"}""");
        result.Value.OccurredAtUtc.Should().Be(occurredAtUtc);
        result.Value.Status.Should().Be(OutboxMessageStatus.Pending);
        result.Value.RetryCount.Should().Be(0);
        result.Value.NextRetryAtUtc.Should().BeNull();
        result.Value.ProcessedAtUtc.Should().BeNull();
        result.Value.Error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyType_ShouldReturnValidationError(string type)
    {
        var result = Create(type: type);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("outbox_message.create.type_required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPayload_ShouldReturnValidationError(string payload)
    {
        var result = Create(payload: payload);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("outbox_message.create.payload_required");
    }

    [Fact]
    public void Create_WithDefaultOccurredAt_ShouldReturnValidationError()
    {
        var result = OutboxMessage.Create(
            "Messenger.ChatMessageCreated",
            """{"messageId":"message-id"}""",
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("outbox_message.create.occurred_at_required");
    }

    [Fact]
    public void MarkProcessed_FromPending_ShouldMarkProcessed()
    {
        var message = Create().Value;
        var processedAtUtc = DateTime.UtcNow;

        var result = message.MarkProcessed(processedAtUtc);

        result.IsSuccess.Should().BeTrue();
        message.Status.Should().Be(OutboxMessageStatus.Processed);
        message.ProcessedAtUtc.Should().Be(processedAtUtc);
        message.NextRetryAtUtc.Should().BeNull();
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkProcessed_FromFailed_ShouldReturnConflict()
    {
        var message = Create().Value;
        message.MarkFailedOrRetry(DateTime.UtcNow, maxAttempts: 1, TimeSpan.Zero, "Unknown type");

        var result = message.MarkProcessed(DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("outbox_message.already_failed");
    }

    [Fact]
    public void MarkFailedOrRetry_BeforeMaxAttempts_ShouldScheduleRetry()
    {
        var message = Create().Value;
        var utcNow = DateTime.UtcNow;

        var result = message.MarkFailedOrRetry(
            utcNow,
            maxAttempts: 3,
            TimeSpan.FromSeconds(30),
            "SignalR unavailable");

        result.IsSuccess.Should().BeTrue();
        message.Status.Should().Be(OutboxMessageStatus.Pending);
        message.RetryCount.Should().Be(1);
        message.NextRetryAtUtc.Should().Be(utcNow.AddSeconds(30));
        message.Error.Should().Be("SignalR unavailable");
    }

    [Fact]
    public void MarkFailedOrRetry_WhenMaxAttemptsReached_ShouldMarkFailed()
    {
        var message = Create().Value;

        var result = message.MarkFailedOrRetry(
            DateTime.UtcNow,
            maxAttempts: 1,
            TimeSpan.FromSeconds(30),
            "Unknown type");

        result.IsSuccess.Should().BeTrue();
        message.Status.Should().Be(OutboxMessageStatus.Failed);
        message.RetryCount.Should().Be(1);
        message.NextRetryAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkFailedOrRetry_FromProcessed_ShouldReturnConflict()
    {
        var message = Create().Value;
        message.MarkProcessed(DateTime.UtcNow);

        var result = message.MarkFailedOrRetry(
            DateTime.UtcNow,
            maxAttempts: 3,
            TimeSpan.FromSeconds(30),
            "failure");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("outbox_message.already_processed");
    }

    [Fact]
    public void MarkFailedOrRetry_WithLongError_ShouldTruncateError()
    {
        var message = Create().Value;
        var error = new string('a', OutboxMessage.MaxErrorLength + 20);

        message.MarkFailedOrRetry(
            DateTime.UtcNow,
            maxAttempts: 3,
            TimeSpan.FromSeconds(30),
            error);

        message.Error.Should().HaveLength(OutboxMessage.MaxErrorLength);
    }

    private static FitLead.Common.Results.Result<OutboxMessage> Create(
        string type = "Messenger.ChatMessageCreated",
        string payload = """{"messageId":"message-id"}""",
        DateTime? occurredAtUtc = null)
    {
        return OutboxMessage.Create(
            type,
            payload,
            occurredAtUtc ?? DateTime.UtcNow);
    }
}
