using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
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
}
