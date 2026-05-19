using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Users;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Api.Identity
{
    public static class DemoMessengerSeeder
    {
        private const string TrainerEmail = "demo.trainer@fitlead.local";
        private const string ClientEmail = "demo.client@fitlead.local";
        private const string Password = "Demo123!";
        private const int TargetMessageCount = 150;

        public static async Task SeedAsync(
            IServiceProvider services,
            CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

            await EnsureRoleAsync(roleManager, "Trainer");
            await EnsureRoleAsync(roleManager, "Client");

            var trainer = await EnsureDemoUserAsync(
                userManager,
                dbContext,
                TrainerEmail,
                "Demo Trainer",
                "Trainer",
                UserRole.Trainer,
                cancellationToken);

            var client = await EnsureDemoUserAsync(
                userManager,
                dbContext,
                ClientEmail,
                "Demo Client",
                "Client",
                UserRole.Client,
                cancellationToken);

            await EnsureRelationshipAsync(dbContext, trainer.Id, client.Id, cancellationToken);
            var chat = await EnsureChatAsync(dbContext, trainer.Id, client.Id, cancellationToken);
            await EnsureMessagesAsync(dbContext, chat, trainer.Id, client.Id, cancellationToken);
        }

        private static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName)
        {
            var existing = await roleManager.FindByNameAsync(roleName);
            if (existing is not null)
            {
                return;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });

            if (!result.Succeeded)
            {
                ThrowIdentityError($"Failed to seed role '{roleName}'.", result);
            }
        }

        private static async Task<User> EnsureDemoUserAsync(
            UserManager<AppIdentityUser> userManager,
            FitLeadDbContext dbContext,
            string email,
            string fullName,
            string roleName,
            UserRole role,
            CancellationToken cancellationToken)
        {
            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser is null)
            {
                identityUser = new AppIdentityUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(identityUser, Password);
                if (!createResult.Succeeded)
                {
                    ThrowIdentityError($"Failed to seed demo user '{email}'.", createResult);
                }
            }

            if (!await userManager.IsInRoleAsync(identityUser, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(identityUser, roleName);
                if (!roleResult.Succeeded)
                {
                    ThrowIdentityError($"Failed to assign role '{roleName}' to '{email}'.", roleResult);
                }
            }

            return await EnsureDomainUserLinkAsync(
                dbContext,
                identityUser,
                email,
                fullName,
                role,
                cancellationToken);
        }

        private static async Task<User> EnsureDomainUserLinkAsync(
            FitLeadDbContext dbContext,
            AppIdentityUser identityUser,
            string email,
            string fullName,
            UserRole role,
            CancellationToken cancellationToken)
        {
            var identityLink = await dbContext.UserIdentityLinks
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdentityUserId == identityUser.Id, cancellationToken);

            if (identityLink is not null)
            {
                return await dbContext.DomainUsers
                    .SingleAsync(x => x.Id == identityLink.DomainUserId, cancellationToken);
            }

            var domainUser = await dbContext.DomainUsers
                .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

            if (domainUser is null)
            {
                var createResult = role == UserRole.Trainer
                    ? User.CreateTrainer(email, fullName)
                    : User.CreateClient(email, fullName);

                if (createResult.IsFailure)
                {
                    throw new InvalidOperationException(createResult.Error.Message);
                }

                domainUser = createResult.Value;
                dbContext.DomainUsers.Add(domainUser);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var domainLink = await dbContext.UserIdentityLinks
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.DomainUserId == domainUser.Id, cancellationToken);

            if (domainLink is not null && domainLink.IdentityUserId != identityUser.Id)
            {
                throw new InvalidOperationException(
                    $"Domain user '{domainUser.Id}' is already linked to another identity user.");
            }

            if (domainLink is null)
            {
                dbContext.UserIdentityLinks.Add(new UserIdentityLink(domainUser.Id, identityUser.Id));
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return domainUser;
        }

        private static async Task EnsureRelationshipAsync(
            FitLeadDbContext dbContext,
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var exists = await dbContext.TrainerClients
                .AnyAsync(
                    x => x.TrainerId == trainerId && x.ClientId == clientId,
                    cancellationToken);

            if (exists)
            {
                return;
            }

            dbContext.TrainerClients.Add(new TrainerClient(trainerId, clientId));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task<Chat> EnsureChatAsync(
            FitLeadDbContext dbContext,
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var chat = await dbContext.Chats
                .SingleOrDefaultAsync(
                    x => x.TrainerId == trainerId && x.ClientId == clientId,
                    cancellationToken);

            if (chat is not null)
            {
                return chat;
            }

            var createResult = Chat.Create(
                trainerId,
                clientId,
                DateTime.UtcNow.AddHours(-3));

            if (createResult.IsFailure)
            {
                throw new InvalidOperationException(createResult.Error.Message);
            }

            chat = createResult.Value;
            dbContext.Chats.Add(chat);
            await dbContext.SaveChangesAsync(cancellationToken);

            return chat;
        }

        private static async Task EnsureMessagesAsync(
            FitLeadDbContext dbContext,
            Chat chat,
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var existingCount = await dbContext.ChatMessages
                .CountAsync(x => x.ChatId == chat.Id, cancellationToken);

            if (existingCount >= TargetMessageCount)
            {
                await EnsureLastMessageAtUtcAsync(dbContext, chat, cancellationToken);
                return;
            }

            var lastCreatedAtUtc = await dbContext.ChatMessages
                .Where(x => x.ChatId == chat.Id)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => (DateTime?)x.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            var nextCreatedAtUtc = lastCreatedAtUtc?.AddMinutes(1)
                ?? DateTime.UtcNow.AddMinutes(-TargetMessageCount);

            for (var index = existingCount + 1; index <= TargetMessageCount; index++)
            {
                var senderId = index % 2 == 0 ? clientId : trainerId;
                var text = CreateMessageText(index, senderId == trainerId);
                var createResult = ChatMessage.CreateText(
                    chat,
                    senderId,
                    text,
                    nextCreatedAtUtc);

                if (createResult.IsFailure)
                {
                    throw new InvalidOperationException(createResult.Error.Message);
                }

                dbContext.ChatMessages.Add(createResult.Value);
                chat.MarkMessageCreated(nextCreatedAtUtc);
                nextCreatedAtUtc = nextCreatedAtUtc.AddMinutes(1);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureLastMessageAtUtcAsync(
            FitLeadDbContext dbContext,
            Chat chat,
            CancellationToken cancellationToken)
        {
            if (chat.LastMessageAtUtc.HasValue)
            {
                return;
            }

            var lastCreatedAtUtc = await dbContext.ChatMessages
                .Where(x => x.ChatId == chat.Id)
                .MaxAsync(x => (DateTime?)x.CreatedAtUtc, cancellationToken);

            if (lastCreatedAtUtc.HasValue)
            {
                chat.MarkMessageCreated(lastCreatedAtUtc.Value);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private static string CreateMessageText(int index, bool isTrainer)
        {
            if (index % 15 == 0)
            {
                return isTrainer
                    ? $"Повідомлення {index}: сьогодні зверніть увагу на техніку, темп і відпочинок між підходами. Якщо відчуття будуть важкими, зменшимо навантаження."
                    : $"Повідомлення {index}: тренування пройшло нормально, але в останніх підходах було складніше тримати темп. Записав відчуття після вправ.";
            }

            return isTrainer
                ? $"Повідомлення {index}: як самопочуття після тренування?"
                : $"Повідомлення {index}: все добре, готовий рухатися далі.";
        }

        private static void ThrowIdentityError(string message, IdentityResult result)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => $"{error.Code}:{error.Description}"));

            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}
