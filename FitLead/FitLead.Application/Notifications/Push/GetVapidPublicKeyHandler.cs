using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Push
{
    public sealed class GetVapidPublicKeyHandler
        : IRequestHandler<GetVapidPublicKeyQuery, Result<VapidPublicKeyDto>>
    {
        private readonly IPushVapidConfiguration _configuration;

        public GetVapidPublicKeyHandler(IPushVapidConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<Result<VapidPublicKeyDto>> Handle(
            GetVapidPublicKeyQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_configuration.PublicKey))
            {
                return Task.FromResult(
                    Result<VapidPublicKeyDto>.Failure(
                        Error.Failure("push.vapid_public_key_missing", "VAPID public key is not configured")));
            }

            return Task.FromResult(
                Result<VapidPublicKeyDto>.Success(
                    new VapidPublicKeyDto(_configuration.PublicKey)));
        }
    }
}
