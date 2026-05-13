using FitLead.Domain.Messenger.Chats;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Messenger;

public sealed class ChatTests
{
    [Fact]
    public void Create_WithValidTrainerAndClient_ShouldCreateChat()
    {
        var trainerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var result = Chat.Create(trainerId, clientId, createdAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.TrainerId.Should().Be(trainerId);
        result.Value.ClientId.Should().Be(clientId);
        result.Value.CreatedAtUtc.Should().Be(createdAtUtc);
        result.Value.LastMessageAtUtc.Should().BeNull();
        result.Value.HasParticipant(trainerId).Should().BeTrue();
        result.Value.HasParticipant(clientId).Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTrainerId_ShouldReturnValidationError()
    {
        var result = Chat.Create(Guid.Empty, Guid.NewGuid(), DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("chat.create.trainer_id_required");
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldReturnValidationError()
    {
        var result = Chat.Create(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("chat.create.client_id_required");
    }
}
