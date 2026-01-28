using FitLead.Application.Common.Results;
using FitLead.Domain.Common.Exceptions;
using MediatR;


namespace FitLead.Application.Common.Results
{
    public sealed class DomainExceptionToResultBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
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
                // Result (non-generic)
                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(ex.Message);
                }

                // Result<T>
                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var failureMethod = typeof(Result<>)
                        .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                        .GetMethod(nameof(Result<object>.Failure))!;

                    return (TResponse)failureMethod.Invoke(null, new object[] { ex.Message })!;
                }

                throw;
            }
        }
    }
}