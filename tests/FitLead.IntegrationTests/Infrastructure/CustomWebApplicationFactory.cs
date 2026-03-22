using FitLead.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace FitLead.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    private static readonly (string PrivateKeyPem, string PublicKeyPem) RsaKeys = CreateTestRsaKeys();
    private readonly DatabaseCheckpoint _databaseCheckpoint = new();

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        await dbContext.Database.MigrateAsync();

        await _databaseCheckpoint.InitializeAsync(connectionString);
        await _databaseCheckpoint.ResetAsync();
        await EnsureRolesAsync(scope.ServiceProvider);
    }

    public Task ResetDatabaseAsync() => _databaseCheckpoint.ResetAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:RsaPrivateKeyPem"] = EscapePem(RsaKeys.PrivateKeyPem),
                ["Jwt:RsaPublicKeyPem"] = EscapePem(RsaKeys.PublicKeyPem)
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _databaseCheckpoint.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        base.Dispose(disposing);
    }

    private static async Task EnsureRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await EnsureRoleExistsAsync(roleManager, "Trainer");
        await EnsureRoleExistsAsync(roleManager, "Client");
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (result.Succeeded)
            return;

        var errors = string.Join("; ", result.Errors.Select(x => $"{x.Code}:{x.Description}"));
        throw new InvalidOperationException($"Failed to create role '{roleName}'. {errors}");
    }

    private static (string PrivateKeyPem, string PublicKeyPem) CreateTestRsaKeys()
    {
        using var rsa = RSA.Create(2048);
        return (rsa.ExportRSAPrivateKeyPem(), rsa.ExportRSAPublicKeyPem());
    }

    private static string EscapePem(string pem)
        => pem.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
}
