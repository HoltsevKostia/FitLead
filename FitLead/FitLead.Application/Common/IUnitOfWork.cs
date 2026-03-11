namespace FitLead.Application.Common
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
