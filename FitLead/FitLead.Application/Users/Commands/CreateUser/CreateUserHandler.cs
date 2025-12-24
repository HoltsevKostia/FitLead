using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Commands.CreateUser
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
            var exists = await _userRepository.ExistsByEmailAsync(
                request.Email,
                cancellationToken);

            if (exists)
                return Result<Guid>.Failure("User with this email already exists");

            User user = request.Role switch
            {
                UserRole.Trainer => User.CreateTrainer(
                    request.Email,
                    request.FullName),

                UserRole.Client => User.CreateClient(
                    request.Email,
                    request.FullName),

                _ => throw new InvalidOperationException("Unsupported role")
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(user.Id);
        }
    }
}
