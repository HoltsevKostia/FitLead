using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Messenger;

public sealed class ChatMessageTests
{
    [Fact]
    public void CreateText_WithParticipantAndText_ShouldCreateTextMessage()
    {
        var trainerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var chat = Chat.Create(trainerId, clientId, DateTime.UtcNow).Value;
        var createdAtUtc = DateTime.UtcNow;

        var result = ChatMessage.CreateText(chat, trainerId, "  Hello  ", createdAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.ChatId.Should().Be(chat.Id);
        result.Value.SenderId.Should().Be(trainerId);
        result.Value.Type.Should().Be(ChatMessageType.Text);
        result.Value.Text.Should().Be("Hello");
        result.Value.VideoReportId.Should().BeNull();
        result.Value.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void CreateText_WithNonParticipantSender_ShouldReturnValidationError()
    {
        var chat = Chat.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;

        var result = ChatMessage.CreateText(
            chat,
            Guid.NewGuid(),
            "Hello",
            DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("chat_message.create.sender_not_participant");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateText_WithEmptyText_ShouldReturnValidationError(string text)
    {
        var trainerId = Guid.NewGuid();
        var chat = Chat.Create(trainerId, Guid.NewGuid(), DateTime.UtcNow).Value;

        var result = ChatMessage.CreateText(
            chat,
            trainerId,
            text,
            DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("chat_message.create.text_required");
    }

    [Fact]
    public void CreateText_WithTooLongText_ShouldReturnValidationError()
    {
        var trainerId = Guid.NewGuid();
        var chat = Chat.Create(trainerId, Guid.NewGuid(), DateTime.UtcNow).Value;
        var text = new string('a', ChatMessage.MaxTextLength + 1);

        var result = ChatMessage.CreateText(
            chat,
            trainerId,
            text,
            DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("chat_message.create.text_too_long");
    }

    [Fact]
    public void CreateVideoReport_WithMatchingChat_ShouldCreateReportMessage()
    {
        var trainerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var chat = Chat.Create(trainerId, clientId, DateTime.UtcNow).Value;
        var report = CreateVideoReport(chat.Id, clientId, trainerId);
        var createdAtUtc = DateTime.UtcNow;

        var result = ChatMessage.CreateVideoReport(
            chat,
            report,
            clientId,
            createdAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ChatMessageType.VideoReport);
        result.Value.Text.Should().BeNull();
        result.Value.VideoReportId.Should().Be(report.Id);
    }

    [Fact]
    public void CreateVideoReport_WithReportFromAnotherChat_ShouldReturnValidationError()
    {
        var trainerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var chat = Chat.Create(trainerId, clientId, DateTime.UtcNow).Value;
        var report = CreateVideoReport(Guid.NewGuid(), clientId, trainerId);

        var result = ChatMessage.CreateVideoReport(
            chat,
            report,
            clientId,
            DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("chat_message.create.video_report_chat_mismatch");
    }

    private static VideoReport CreateVideoReport(
        Guid chatId,
        Guid clientId,
        Guid trainerId)
    {
        return VideoReport.Create(
            chatId,
            clientId,
            trainerId,
            "Squat check",
            null,
            [Guid.NewGuid()],
            DateTime.UtcNow).Value;
    }
}
