namespace FitLead.Application.Trainings.TrainingProgramAssignments.Outbox
{
    public sealed record TrainingProgramAssignedOutboxPayload(
        Guid AssignmentId,
        Guid TrainingProgramId,
        Guid TrainerId,
        Guid ClientId,
        string ProgramTitle,
        DateTime AssignedAtUtc);
}
