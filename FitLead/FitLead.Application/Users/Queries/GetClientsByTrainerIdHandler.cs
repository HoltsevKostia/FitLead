using FitLead.Application.Abstractions.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetClientsByTrainerIdHandler
    : IRequestHandler<GetClientsByTrainerIdQuery, IReadOnlyList<TrainerClientDto>>
    {
        private readonly ITrainerClientReadRepository _repository;

        public GetClientsByTrainerIdHandler(
            ITrainerClientReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<TrainerClientDto>> Handle(
            GetClientsByTrainerIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetClientsByTrainerIdAsync(
                request.TrainerId,
                cancellationToken);
        }
    }
}
