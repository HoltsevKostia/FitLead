using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;

namespace FitLead.Application.Users.Access
{
    public sealed class CurrentUserLoader : ICurrentUserLoader
    {
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userRepository;

        public CurrentUserLoader(
            IUserContext userContext,
            IUserRepository userRepository)
        {
            _userContext = userContext;
            _userRepository = userRepository;
        }

        public async Task<Result<User>> GetCurrentOrNotFoundAsync(CancellationToken cancellationToken)
        {
            var userId = _userContext.UserIdOrNull;
            if (!userId.HasValue)
            {
                return Result<User>.Failure(
                    Error.Unauthorized("auth.user_missing", "Current user is missing"));
            }

            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user is null)
            {
                return Result<User>.Failure(
                    Error.NotFound("user.not_found", "User not found"));
            }

            return Result<User>.Success(user);
        }
    }
}
