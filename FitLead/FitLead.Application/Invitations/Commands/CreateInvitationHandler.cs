using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Domain.Invitations;
using FitLead.Domain.Users;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed class CreateInvitationHandler 
        : IRequestHandler<CreateInvitationCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationRepository _invitationRepository;

        public CreateInvitationHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IInvitationRepository invitationRepository)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _invitationRepository = invitationRepository;
        }

        public async Task<Result<Guid>> Handle(
            CreateInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _userRepository.GetByIdAsync(request.TrainerId, cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure("Trainer not found");

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure("User is not a trainer");

            var client = await _userRepository.GetByIdAsync(request.ClientId, cancellationToken);

            if (client is null)
                return Result<Guid>.Failure("Client not found");

            if (client.Role != UserRole.Client)
                return Result<Guid>.Failure("User is not a Client");

            var alreadyPending = await _invitationRepository
                .ExistsPendingAsync(
                    request.TrainerId,
                    request.ClientId,
                    cancellationToken);

            if (alreadyPending)
                return Result<Guid>.Failure("Invitation already pending");

            var sentToday = await _invitationRepository
                .CountSentByTrainerForDateAsync(
                    request.TrainerId,
                    request.Now,
                    cancellationToken);

            if (sentToday >= 2)
                return Result<Guid>.Failure("Daily invitation limit reached");

            var invitation = Invitation.Create(
                request.TrainerId,
                request.ClientId,
                request.Now);

            await _invitationRepository.AddAsync(invitation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(invitation.Id);
        }
    }
}
