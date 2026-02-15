using FitLead.Application.Abstractions.Persistence;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetUsersByRoleHandler : IRequestHandler<GetUsersByRoleQuery, Result<IReadOnlyList<UserDto>>>
    {
        private readonly IUserReadRepository _repository;

        public GetUsersByRoleHandler(IUserReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
        {
            var users = await _repository.GetByRoleAsync(request.Role, cancellationToken);
            return Result<IReadOnlyList<UserDto>>.Success(users);
        }
    }
}   

