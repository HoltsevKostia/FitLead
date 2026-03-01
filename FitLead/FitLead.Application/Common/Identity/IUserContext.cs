namespace FitLead.Application.Common.Identity
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        string IdentityUserId { get; }
        string? IdentityUserIdOrNull { get; }

        // temprorary properties, remove after full auth (asp identity, jwt) implementation
        Guid UserId { get; }
        Guid? UserIdOrNull { get; }
    }
}
