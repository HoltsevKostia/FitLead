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
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await EnsureRoleAsync(roleManager, "Trainer");
            await EnsureRoleAsync(roleManager, "Client");

            const string email = "dev@fitlead.local";
            const string password = "DevPass123!";

            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                await EnsureUserInRoleAsync(userManager, existing, "Trainer");
                return;
            }

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

            await EnsureUserInRoleAsync(userManager, user, "Trainer");
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var existing = await roleManager.FindByNameAsync(roleName);
            if (existing is not null)
                return;

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

        private static async Task EnsureUserInRoleAsync(
            UserManager<AppIdentityUser> userManager,
            AppIdentityUser user,
            string roleName)
        {
            if (await userManager.IsInRoleAsync(user, roleName))
                return;

            var result = await userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Failed to assign role '{roleName}' to '{user.Email}'. {errors}");
            }
        }
    }
}
