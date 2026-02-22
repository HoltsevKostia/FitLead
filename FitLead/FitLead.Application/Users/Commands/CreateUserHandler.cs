using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Users.Commands
{
    public sealed class CreateUserHandler
    : IRequestHandler<CreateUserCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Role is not (UserRole.Trainer or UserRole.Client))
                return Result<Guid>.Failure(Error.Validation("user.role_invalid", "Unsupported role"));

            var exists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
            if (exists)
                return Result<Guid>.Failure(Error.Conflict("user.email_exists", "User with this email already exists"));

            var user = request.Role == UserRole.Trainer
                ? User.CreateTrainer(request.Email, request.FullName)
                : User.CreateClient(request.Email, request.FullName);

            if (user.IsFailure)
                return Result<Guid>.Failure(user.Error);

            await _userRepository.AddAsync(user.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(user.Value.Id);
        }
    }
}
