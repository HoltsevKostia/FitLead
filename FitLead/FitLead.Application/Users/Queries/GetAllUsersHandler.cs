using FitLead.Application.Abstractions.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetAllUsersHandler
        : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
    {
        private readonly IUserReadRepository _repository;

        public GetAllUsersHandler(IUserReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
    }
}
