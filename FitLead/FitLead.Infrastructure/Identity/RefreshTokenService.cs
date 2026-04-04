using FitLead.Application.Common.Time;
using FitLead.Application.Identity;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace FitLead.Infrastructure.Identity
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly FitLeadDbContext _dbContext;
        private readonly ITokenHasher _tokenHasher;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IClock _clock;
        private readonly JwtOptions _jwtOptions;

        public RefreshTokenService(
            FitLeadDbContext dbContext,
            ITokenHasher tokenHasher,
            UserManager<AppIdentityUser> userManager,
            IClock clock,
            IOptions<JwtOptions> jwtOptions)
        {
            _dbContext = dbContext;
            _tokenHasher = tokenHasher;
            _userManager = userManager;
            _clock = clock;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<(string RefreshToken, Guid FamilyId)> IssueForLoginAsync(
            string identityUserId,
            CancellationToken cancellationToken)
        {
            var now = _clock.UtcNow;
            var familyId = Guid.NewGuid();
            var refreshToken = GenerateRefreshTokenValue();
            var tokenHash = _tokenHasher.ComputeSha256Base64(refreshToken);

            var tokenRow = new RefreshToken(
                identityUserId,
                tokenHash,
                familyId,
                now,
                now.AddDays(GetRefreshTokenDays()));

            _dbContext.RefreshTokens.Add(tokenRow);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (refreshToken, familyId);
        }

        public async Task<RefreshRotateResult> RotateAsync(
            string refreshToken,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return RefreshRotateResult.Failed(RefreshRotateFailure.Invalid);

            var refreshTokenHash = _tokenHasher.ComputeSha256Base64(refreshToken);
            var now = _clock.UtcNow;

            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var current = await _dbContext.RefreshTokens
                    .FromSqlInterpolated(
                        $@"SELECT * FROM refresh_tokens WHERE ""TokenHash"" = {refreshTokenHash} FOR UPDATE")
                    .SingleOrDefaultAsync(cancellationToken);

                if (current is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return RefreshRotateResult.Failed(RefreshRotateFailure.Invalid);
                }

                var user = await _userManager.FindByIdAsync(current.IdentityUserId);
                if (user is null || IsLockedOut(user, now))
                {
                    await tx.RollbackAsync(cancellationToken);
                    return RefreshRotateResult.Failed(RefreshRotateFailure.UserUnavailable);
                }

                if (current.ExpiresAtUtc <= now)
                {
                    await MarkTokenExpiredIfNeededAsync(current.Id, now, cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    return RefreshRotateResult.Failed(RefreshRotateFailure.Expired);
                }

                if (current.RevokedAtUtc is not null)
                {
                    if (string.Equals(
                        current.ReasonRevoked,
                        RefreshTokenRevocationReasons.Rotated,
                        StringComparison.Ordinal))
                    {
                        await RevokeFamilyInternalAsync(
                            current.FamilyId,
                            now,
                            RefreshTokenRevocationReasons.ReuseDetected,
                            cancellationToken);

                        await tx.CommitAsync(cancellationToken);
                        return RefreshRotateResult.Failed(RefreshRotateFailure.ReuseDetected);
                    }

                    await tx.RollbackAsync(cancellationToken);
                    return RefreshRotateResult.Failed(RefreshRotateFailure.Revoked);
                }

                var revokeAffectedRows = await _dbContext.RefreshTokens
                    .Where(x => x.Id == current.Id && x.RevokedAtUtc == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.RevokedAtUtc, now)
                        .SetProperty(x => x.ReasonRevoked, RefreshTokenRevocationReasons.Rotated), cancellationToken);

                if (revokeAffectedRows != 1)
                {
                    await RevokeFamilyInternalAsync(
                        current.FamilyId,
                        now,
                        RefreshTokenRevocationReasons.ReuseDetected,
                        cancellationToken);

                    await tx.CommitAsync(cancellationToken);
                    return RefreshRotateResult.Failed(RefreshRotateFailure.ReuseDetected);
                }

                var newRefreshToken = GenerateRefreshTokenValue();
                var newRefreshTokenHash = _tokenHasher.ComputeSha256Base64(newRefreshToken);
                var replacement = new RefreshToken(
                    current.IdentityUserId,
                    newRefreshTokenHash,
                    current.FamilyId,
                    now,
                    now.AddDays(GetRefreshTokenDays()));

                _dbContext.RefreshTokens.Add(replacement);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _dbContext.RefreshTokens
                    .Where(x => x.Id == current.Id)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            x => x.ReplacedByTokenId,
                            replacement.Id),
                        cancellationToken);

                await tx.CommitAsync(cancellationToken);

                return RefreshRotateResult.Succeeded(current.IdentityUserId, newRefreshToken);
            }
            catch
            {
                try
                {
                    await tx.RollbackAsync(cancellationToken);
                }
                catch { }
                throw;
            }
        }

        public async Task RevokeFamilyByTokenAsync(
            string refreshToken,
            string reason,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return;

            var tokenHash = _tokenHasher.ComputeSha256Base64(refreshToken);
            var row = await _dbContext.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

            if (row is null)
                return;

            await RevokeFamilyInternalAsync(row.FamilyId, _clock.UtcNow, reason, cancellationToken);
        }

        private async Task MarkTokenExpiredIfNeededAsync(
            Guid tokenId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            await _dbContext.RefreshTokens
                .Where(x => x.Id == tokenId && x.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAtUtc, now)
                    .SetProperty(x => x.ReasonRevoked, RefreshTokenRevocationReasons.Expired), cancellationToken);
        }

        private async Task RevokeFamilyInternalAsync(
            Guid familyId,
            DateTime revokedAtUtc,
            string reason,
            CancellationToken cancellationToken)
        {
            await _dbContext.RefreshTokens
                .Where(x => x.FamilyId == familyId && x.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAtUtc, revokedAtUtc)
                    .SetProperty(x => x.ReasonRevoked, reason), cancellationToken);
        }

        private int GetRefreshTokenDays() => Math.Max(1, _jwtOptions.RefreshTokenDays);

        private static bool IsLockedOut(AppIdentityUser user, DateTime now)
        {
            if (!user.LockoutEnd.HasValue)
                return false;

            return user.LockoutEnd.Value.UtcDateTime > now;
        }

        private static string GenerateRefreshTokenValue()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(bytes);
            return token.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
