namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed class TrainingProgramDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public int WeeksCount { get; init; }
        public int DaysPerWeek { get; init; }
    }
}
