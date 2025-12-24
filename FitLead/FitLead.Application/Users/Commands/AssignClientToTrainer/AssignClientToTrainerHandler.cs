using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Commands.AssignClientToTrainer
{
    public sealed class AssignClientToTrainerHandler
    : IRequestHandler<AssignClientToTrainerCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITrainerClientRepository _trainerClientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignClientToTrainerHandler(
            IUserRepository userRepository,
            ITrainerClientRepository trainerClientRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _trainerClientRepository = trainerClientRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            AssignClientToTrainerCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(
                request.TrainerId,
                cancellationToken);

            if (trainer is null || !trainer.IsTrainer)
                return Result.Failure("Trainer not found");

            var client = await _userRepository.GetByIdAsync(
                request.ClientId,
                cancellationToken);

            if (client is null || !client.IsClient)
                return Result.Failure("Client not found");

            var exists = await _trainerClientRepository.ExistsAsync(
                request.TrainerId,
                request.ClientId,
                cancellationToken);

            if (exists)
                return Result.Failure("Client already assigned to trainer");

            await _trainerClientRepository.AddAsync(
                request.TrainerId,
                request.ClientId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
