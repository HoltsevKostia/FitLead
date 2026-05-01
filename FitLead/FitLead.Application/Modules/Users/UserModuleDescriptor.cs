using FitLead.Domain.Users;

namespace FitLead.Application.Modules.Users
{
    public sealed record UserModuleDescriptor(
        Guid Id,
        UserRole Role);
}
