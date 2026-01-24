using FitLead.Application.Common.Results;
using FitLead.Domain.Common.Exceptions;
using MediatR;

namespace FitLead.Application.Common.Pipeline
{
    public sealed class DomainExceptionToResultBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Result
    {
        private readonly IResultFactory<TResponse> _factory;

        public DomainExceptionToResultBehavior(IResultFactory<TResponse> factory)
        {
            _factory = factory;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (DomainException ex)
            {
                return _factory.Failure(ex.Message);
            }
        }
    }
}
