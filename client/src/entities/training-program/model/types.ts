export interface TrainingProgram {
  id: string;
  title: string;
  weeksCount: number;
  daysPerWeek: number;
}

export interface CreateTrainingProgramRequest {
  title: string;
  weeksCount: number;
  daysPerWeek: number;
}

export interface AddTrainingProgramWorkoutRequest {
  workoutId: string;
  weekNumber: number;
  dayNumber: number;
}

export interface AssignTrainingProgramToClientRequest {
  clientId: string;
  expiresAtUtc: string | null;
}

export interface TrainingProgramAssignment {
  assignmentId: string;
  clientId: string;
  clientName: string;
  status: string;
  accessSource: string;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
  revokedAtUtc: string | null;
}

export interface TrainingProgramWorkout {
  id: string;
  workoutId: string;
  workoutName: string;
  trainerId: string;
  weekNumber: number;
  dayNumber: number;
  orderInDay: number;
}
