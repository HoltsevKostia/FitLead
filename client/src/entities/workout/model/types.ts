import type { Equipment, MuscleGroup } from "@/entities/exercise/model/types";
import type { MediaAssetPreview } from "@/entities/media-asset/model/types";

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
  exerciseMediaAsset: MediaAssetPreview | null;
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

export interface AddWorkoutExerciseRequest {
  exerciseId: string;
  repetitions: number;
  sets: number;
  loadKg: number | null;
  restSeconds: number;
  trainerNote: string | null;
}

export interface UpdateWorkoutExerciseRequest {
  repetitions: number;
  sets: number;
  loadKg: number | null;
  restSeconds: number;
  trainerNote: string | null;
}
