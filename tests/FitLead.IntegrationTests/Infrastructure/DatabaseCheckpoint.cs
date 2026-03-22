using Npgsql;
using Respawn;
using Respawn.Graph;

namespace FitLead.IntegrationTests.Infrastructure;

public sealed class DatabaseCheckpoint : IAsyncDisposable
{
    private Respawner? _respawner;
    private NpgsqlConnection? _connection;

    public async Task InitializeAsync(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore =
            [
                new Table("__EFMigrationsHistory"),
                new Table("AspNetRoles")
            ]
        });
    }

    public async Task ResetAsync()
    {
        if (_respawner is null || _connection is null)
            throw new InvalidOperationException("Database checkpoint is not initialized.");

        await _respawner.ResetAsync(_connection);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
