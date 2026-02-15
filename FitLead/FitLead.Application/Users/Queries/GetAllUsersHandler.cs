using FitLead.Application.Abstractions.Persistence;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetAllUsersHandler
        : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
    {
        private readonly IUserReadRepository _repository;

        public GetAllUsersHandler(IUserReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _repository.GetAllAsync(cancellationToken);
            return Result<IReadOnlyList<UserDto>>.Success(users);
        }
    }
}
