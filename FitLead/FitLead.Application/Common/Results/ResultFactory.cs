namespace FitLead.Application.Common.Results
{
    public sealed class ResultFactory : IResultFactory<Result>
    {
        public Result Failure(string error) => Result.Failure(error);
    }

    public sealed class ResultFactory<T> : IResultFactory<Result<T>>
    {
        public Result<T> Failure(string error) => Result<T>.Failure(error);
    }
}
