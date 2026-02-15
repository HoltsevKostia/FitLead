using FitLead.Common.Errors;
using FitLead.Common.Results;
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
                var error = Error.Failure("domain.exception", ex.Message);

                if (typeof(TResponse) == typeof(Result))
                    return (TResponse)(object)Result.Failure(error);

                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var t = typeof(TResponse).GetGenericArguments()[0];
                    var resultType = typeof(Result<>).MakeGenericType(t);

                    // Find: public static Result<T> Failure(Error error)
                    var failureMethod = resultType.GetMethod(
                        "Failure",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(Error) },
                        modifiers: null);

                    if (failureMethod is null)
                        throw;

                    return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
                }

                throw;
            }
        }
    }
}