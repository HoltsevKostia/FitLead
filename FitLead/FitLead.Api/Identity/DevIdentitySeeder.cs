using FitLead.Domain.Users;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Api.Identity
{
    public static class DevIdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

            await EnsureRoleAsync(roleManager, "Trainer");
            await EnsureRoleAsync(roleManager, "Client");

            const string email = "dev@fitlead.local";
            const string password = "DevPass123!";

            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser is null)
            {
                identityUser = new AppIdentityUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(identityUser, password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Failed to seed dev identity user. {errors}");
                }
            }

            await EnsureUserInRoleAsync(userManager, identityUser, "Trainer");
            await EnsureDomainUserLinkAsync(dbContext, identityUser, email, ct);
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

        private static async Task EnsureDomainUserLinkAsync(
            FitLeadDbContext dbContext,
            AppIdentityUser identityUser,
            string email,
            CancellationToken ct)
        {
            var identityLink = await dbContext.UserIdentityLinks
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdentityUserId == identityUser.Id, ct);

            if (identityLink is not null)
                return;

            var domainUser = await dbContext.DomainUsers
                .SingleOrDefaultAsync(x => x.Email == email, ct);

            if (domainUser is null)
            {
                var createDomainResult = User.CreateTrainer(email, "Dev Trainer");
                if (createDomainResult.IsFailure)
                    throw new InvalidOperationException(createDomainResult.Error.Message);

                domainUser = createDomainResult.Value;
                dbContext.DomainUsers.Add(domainUser);
                await dbContext.SaveChangesAsync(ct);
            }

            var domainLink = await dbContext.UserIdentityLinks
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.DomainUserId == domainUser.Id, ct);

            if (domainLink is null)
            {
                dbContext.UserIdentityLinks.Add(
                    new UserIdentityLink(domainUser.Id, identityUser.Id));
                await dbContext.SaveChangesAsync(ct);
                return;
            }

            if (domainLink.IdentityUserId != identityUser.Id)
            {
                throw new InvalidOperationException(
                    $"Domain user '{domainUser.Id}' is already linked to another identity user.");
            }
        }
    }
}
