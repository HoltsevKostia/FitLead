import type { Equipment, MuscleGroup } from "@/entities/exercise/model/types";

export interface Workout {
  id: string;
  name: string;
  trainerId: string;
}

export interface WorkoutExerciseDetails {
  workoutExerciseId: string;
  exerciseId: string;
  order: number;
  exerciseName: string;
  exerciseDescription: string;
  exerciseMediaUrl: string | null;
  exerciseMuscleGroup: MuscleGroup | null;
  exerciseEquipment: Equipment | null;
  repetitions: number;
  sets: number;
  loadKg: number | null;
  restSeconds: number;
  trainerNote: string | null;
}

export interface WorkoutDetails extends Workout {
  exercises: WorkoutExerciseDetails[];
}

export interface CreateWorkoutRequest {
  name: string;
}
