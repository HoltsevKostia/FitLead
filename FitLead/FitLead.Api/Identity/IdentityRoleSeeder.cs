using Microsoft.AspNetCore.Identity;

namespace FitLead.Api.Identity
{
    public static class IdentityRoleSeeder
    {
        public const string TrainerRole = "Trainer";
        public const string ClientRole = "Client";

        public static async Task SeedAsync(
            IServiceProvider services,
            CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(IdentityRoleSeeder));

            await EnsureRoleAsync(roleManager, TrainerRole, cancellationToken);
            await EnsureRoleAsync(roleManager, ClientRole, cancellationToken);

            logger.LogInformation("Identity role seeding completed.");
        }

        public static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existing = await roleManager.FindByNameAsync(roleName);
            if (existing is not null)
            {
                return;
            }

            var role = new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Failed to seed role '{roleName}'. {errors}");
            }
        }
    }
}
