using FitLead.Common.Errors;

namespace FitLead.Common.Results
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None ||
                !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static Result Success() =>
            new(true, Error.None);

        public static Result Failure(Error error)
        {
            if (error == Error.None)
                throw new ArgumentException("Cannot use Error.None for failure.", nameof(error));

            return new(false, error);
        }
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        private Result(T? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public T Value =>
            IsSuccess
                ? _value!
                : throw new InvalidOperationException(
                    "Cannot access value of a failure result.");

        public static Result<T> Success(T value) =>
            new(value ?? throw new ArgumentNullException(nameof(value)),
                true,
                Error.None);

        public new static Result<T> Failure(Error error) =>
            new(default, false, error);
    }
}
