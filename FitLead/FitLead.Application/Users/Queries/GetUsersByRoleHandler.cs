using FitLead.Application.Abstractions.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetUsersByRoleHandler : IRequestHandler<GetUsersByRoleQuery, IReadOnlyList<UserDto>>
    {
        private readonly IUserReadRepository _repository;

        public GetUsersByRoleHandler(IUserReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<UserDto>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByRoleAsync(request.Role, cancellationToken);
        }
    }
}   

