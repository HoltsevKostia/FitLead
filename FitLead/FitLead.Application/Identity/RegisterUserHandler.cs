using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace FitLead.Application.Identity
{
    public sealed class RegisterUserHandler
        : IRequestHandler<RegisterUserCommand, Result<AuthTokensResult>>
    {
        private readonly IIdentityAccountService _identityProvisioner;
        private readonly IUserRepository _userRepository;
        private readonly IUserIdentityLinkWriter _linkWriter;
        private readonly IAuthTokenIssuer _tokenIssuer;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserHandler(
            IIdentityAccountService identityProvisioner,
            IUserRepository userRepository,
            IUserIdentityLinkWriter linkWriter,
            IAuthTokenIssuer tokenIssuer,
            IUnitOfWork unitOfWork)
        {
            _identityProvisioner = identityProvisioner;
            _userRepository = userRepository;
            _linkWriter = linkWriter;
            _tokenIssuer = tokenIssuer;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthTokensResult>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            var validationError = ValidateRequest(request);
            if (validationError is not null)
                return Result<AuthTokensResult>.Failure(validationError);

            var roleMapResult = TryMapRole(request.Role);
            if (roleMapResult.IsFailure)
                return Result<AuthTokensResult>.Failure(roleMapResult.Error);

            var roleMap = roleMapResult.Value;
            var normalizedEmail = request.Email.Trim();
            var normalizedFullName = request.FullName.Trim();

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var emailExists = await _identityProvisioner.ExistsByEmailAsync(
                    normalizedEmail,
                    cancellationToken);
                if (emailExists)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Result<AuthTokensResult>.Failure(
                        Error.Conflict("auth.email_exists", "User with this email already exists"));
                }

                var provisionResult = await _identityProvisioner.CreateWithRoleAsync(
                    normalizedEmail,
                    request.Password,
                    roleMap.IdentityRole,
                    cancellationToken);
                if (provisionResult.IsFailure)
                {
                    return Result<AuthTokensResult>.Failure(provisionResult.Error);
                }

                var domainUserResult = roleMap.DomainRole == UserRole.Trainer
                    ? User.CreateTrainer(normalizedEmail, normalizedFullName)
                    : User.CreateClient(normalizedEmail, normalizedFullName);

                if (domainUserResult.IsFailure)
                {
                    return Result<AuthTokensResult>.Failure(domainUserResult.Error);
                }

                var domainUser = domainUserResult.Value;
                await _userRepository.AddAsync(domainUser, cancellationToken);

                var linkResult = await _linkWriter.AddAsync(
                    domainUser.Id,
                    provisionResult.Value.IdentityUserId,
                    cancellationToken);
                if (linkResult.IsFailure)
                {
                    return Result<AuthTokensResult>.Failure(linkResult.Error);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);

                var tokensResult = await _tokenIssuer.IssueAsync(
                    provisionResult.Value.IdentityUserId,
                    roleMap.IdentityRole,
                    cancellationToken);
                if (tokensResult.IsFailure)
                    return Result<AuthTokensResult>.Failure(tokensResult.Error);

                return Result<AuthTokensResult>.Success(tokensResult.Value);
            }
            catch
            {
                try
                {
                    await tx.RollbackAsync(cancellationToken);
                }
                catch
                {
                }

                throw;
            }
        }

        private static Result<RoleMap> TryMapRole(string rawRole)
        {
            if (string.IsNullOrWhiteSpace(rawRole))
                return Result<RoleMap>.Failure(
                    Error.Validation("auth.role_required", "Role is required"));

            var normalized = rawRole.Trim();
            if (string.Equals(normalized, "Trainer", StringComparison.OrdinalIgnoreCase))
                return Result<RoleMap>.Success(new RoleMap(UserRole.Trainer, "Trainer"));

            if (string.Equals(normalized, "Client", StringComparison.OrdinalIgnoreCase))
                return Result<RoleMap>.Success(new RoleMap(UserRole.Client, "Client"));

            return Result<RoleMap>.Failure(
                Error.Validation("auth.role_invalid", "Role must be Trainer or Client"));
        }

        private static Error? ValidateRequest(RegisterUserCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Error.Validation("auth.email_required", "Email is required");
            }

            var email = request.Email.Trim();
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                return Error.Validation("auth.email_invalid", "Email format is invalid");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Error.Validation("auth.password_required", "Password is required");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Error.Validation("auth.full_name_required", "Full name is required");
            }

            return null;
        }

        private sealed record RoleMap(UserRole DomainRole, string IdentityRole);
    }
}
