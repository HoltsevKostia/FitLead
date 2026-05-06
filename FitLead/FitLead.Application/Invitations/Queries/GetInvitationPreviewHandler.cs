using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Invitations.Services;
using FitLead.Application.Modules.Users;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetInvitationPreviewHandler
        : IRequestHandler<GetInvitationPreviewQuery, Result<InvitationPreviewDto>>
    {
        private readonly IClock _clock;
        private readonly IUsersModule _usersModule;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IInvitationLinkService _invitationLinkService;

        public GetInvitationPreviewHandler(
            IClock clock,
            IUsersModule usersModule,
            IInvitationRepository invitationRepository,
            IInvitationLinkService invitationLinkService)
        {
            _clock = clock;
            _usersModule = usersModule;
            _invitationRepository = invitationRepository;
            _invitationLinkService = invitationLinkService;
        }

        public async Task<Result<InvitationPreviewDto>> Handle(
            GetInvitationPreviewQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Result<InvitationPreviewDto>.Failure(
                    Error.Validation("invitation.preview.token_required", "Token is required"));
            }

            var tokenHash = _invitationLinkService.ComputeTokenHash(request.Token.Trim());
            var invitation = await _invitationRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

            if (invitation is null)
            {
                return Result<InvitationPreviewDto>.Failure(
                    Error.NotFound("invitation.not_found", "Invitation not found"));
            }

            var trainerProfile = await _usersModule.GetTrainerPublicProfileAsync(
                invitation.TrainerId,
                cancellationToken);

            if (trainerProfile is null)
            {
                return Result<InvitationPreviewDto>.Failure(
                    Error.NotFound("trainer.not_found", "Trainer not found"));
            }

            var (status, isJoinable) = GetPreviewState(invitation, _clock.UtcNow);

            return Result<InvitationPreviewDto>.Success(
                new InvitationPreviewDto
                {
                    Status = status,
                    IsJoinable = isJoinable,
                    ExpiresAtUtc = invitation.ExpiresAtUtc,
                    Trainer = new InvitationTrainerPreviewDto
                    {
                        FullName = trainerProfile.FullName
                    }
                });
        }

        private static (string Status, bool IsJoinable) GetPreviewState(
            Domain.Invitations.Invitation invitation,
            DateTime nowUtc)
        {
            if (invitation.Status == Domain.Invitations.InvitationStatus.Accepted)
            {
                return ("Accepted", false);
            }

            if (invitation.Status == Domain.Invitations.InvitationStatus.Revoked)
            {
                return ("Revoked", false);
            }

            if (invitation.IsExpired(nowUtc))
            {
                return ("Expired", false);
            }

            return ("Pending", true);
        }
    }
}
