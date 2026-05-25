import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";

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

export type WorkoutLogStatus = "Completed" | "Skipped";

export type LogWorkoutRequest =
  | {
      status: "Completed";
      difficultyRating?: number;
      clientNote?: string;
    }
  | {
      status: "Skipped";
      clientNote?: string;
    };

export interface WorkoutLogPreview {
  id: string;
  status: string;
  performedAtUtc: string | null;
  clientNote: string | null;
  difficultyRating: number | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface ClientAssignedTrainingProgram {
  assignmentId: string;
  programId: string;
  title: string;
  trainerId: string;
  trainerName: string;
  weeksCount: number;
  daysPerWeek: number;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
}

export interface ClientAssignedTrainingProgramWorkout extends TrainingProgramWorkout {
  log: WorkoutLogPreview | null;
  exercises: WorkoutExerciseDetails[];
}

export interface ClientAssignedTrainingProgramDetails
  extends Omit<ClientAssignedTrainingProgram, "programId"> {
  programId: string;
  workouts: ClientAssignedTrainingProgramWorkout[];
}
