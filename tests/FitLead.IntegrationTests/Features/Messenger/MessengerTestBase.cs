using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;

namespace FitLead.IntegrationTests.Features.Messenger;

public abstract class MessengerTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected MessengerTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }

    protected async Task CreateRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await Db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }

    protected async Task<Chat> CreateChatAsync(Guid trainerId, Guid clientId)
    {
        var chat = Chat.Create(trainerId, clientId, DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();
        });

        return chat;
    }

    protected async Task<ChatMessage> CreateTextMessageAsync(
        Chat chat,
        Guid senderId,
        string text,
        DateTime createdAtUtc)
    {
        var message = ChatMessage.CreateText(
            chat,
            senderId,
            text,
            createdAtUtc).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.ChatMessages.AddAsync(message);
            await context.SaveChangesAsync();
        });

        return message;
    }

    protected async Task<MediaAsset> CreateMediaAssetAsync(
        Guid ownerUserId,
        MediaAssetKind kind,
        string contentType,
        int? durationSeconds = 12)
    {
        var mediaAsset = MediaAsset.Create(
            ownerUserId,
            MediaStorageProvider.Uploadcare,
            Guid.NewGuid().ToString(),
            $"https://ucarecdn.example/{Guid.NewGuid():D}/",
            "file.bin",
            contentType,
            1024,
            kind,
            durationSeconds,
            DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.MediaAssets.AddAsync(mediaAsset);
            await context.SaveChangesAsync();
        });

        return mediaAsset;
    }
}
