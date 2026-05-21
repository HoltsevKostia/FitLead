using System.Text.Json;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly TestDb _db;

    public TestOutbox(TestDb db)
    {
        _db = db;
    }

    public async Task<OutboxMessage> GetSingleAsync<TPayload>(
        string type,
        Func<TPayload, bool> payloadPredicate)
    {
        return await _db.QueryAsync(async context =>
        {
            var messages = await context.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Type == type)
                .ToListAsync();

            return messages.Single(message =>
            {
                var payload = JsonSerializer.Deserialize<TPayload>(
                    message.Payload,
                    SerializerOptions);

                return payload is not null && payloadPredicate(payload);
            });
        });
    }
}
