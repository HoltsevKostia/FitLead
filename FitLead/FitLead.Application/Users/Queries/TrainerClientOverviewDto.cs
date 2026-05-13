namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientOverviewDto(
        Guid ClientId,
        string Email,
        string FullName,
        IReadOnlyList<TrainerClientProgramAccessDto> ActivePrograms);
}
