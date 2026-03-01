using FitLead.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FitLead.Api.Identity
{
    public static class DevIdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();

            const string email = "dev@fitlead.local";
            const string password = "DevPass123!";

            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
                return;

            var user = new AppIdentityUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Failed to seed dev identity user. {errors}");
            }
        }
    }
}
