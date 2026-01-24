namespace FitLead.Application.Common.Results
{
    public interface IResultFactory<TResponse>
    {
        TResponse Failure(string error);
    }
}
