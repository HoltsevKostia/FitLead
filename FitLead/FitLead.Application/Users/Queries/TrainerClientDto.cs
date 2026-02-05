namespace FitLead.Application.Users.Queries
{
    public sealed class TrainerClientDto
    {
        public Guid ClientId { get; init; }
        public string Email { get; init; } = null!;
        public string FullName { get; init; } = null!;
    }
}
