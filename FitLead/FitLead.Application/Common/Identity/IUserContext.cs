namespace FitLead.Application.Common.Identity
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        Guid UserId { get; }      
        Guid? UserIdOrNull { get; } 
    }
}
