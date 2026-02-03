using FitLead.Application.Common.Errors;

namespace FitLead.Application.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, null);

        public static Result Failure(Error error)
            => new(false, error ?? throw new ArgumentNullException(nameof(error)));

        // old method needed for build
        public static Result Failure(string message)
            => Failure(Error.Failure("failure", message));
    }

    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool isSuccess, T? value, Error? error)
            : base(isSuccess, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, null);

        public static new Result<T> Failure(Error error)
            => new(false, default, error ?? throw new ArgumentNullException(nameof(error)));

        // old method needed for build
        public static new Result<T> Failure(string message)
            => Failure(Error.Failure("failure", message));
    }
}
