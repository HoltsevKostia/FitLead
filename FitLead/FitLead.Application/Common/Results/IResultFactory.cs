namespace FitLead.Application.Common.Results
{
    public interface IResultFactory<TResponse>
        where TResponse : Result
    {
        TResponse Failure(string error);
    }
}
