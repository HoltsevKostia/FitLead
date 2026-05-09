export enum ExerciseSource {
  Platform = 1,
  Trainer = 2,
}

export enum MuscleGroup {
  Chest = 1,
  Back = 2,
  Shoulders = 3,
  Biceps = 4,
  Triceps = 5,
  Legs = 6,
  Glutes = 7,
  Core = 8,
  FullBody = 9,
  Cardio = 10,
}

export enum Equipment {
  Bodyweight = 1,
  Dumbbells = 2,
  Barbell = 3,
  Kettlebell = 4,
  Machine = 5,
  Cable = 6,
  ResistanceBand = 7,
  Bench = 8,
  PullUpBar = 9,
  Other = 10,
}

export interface Exercise {
  id: string;
  name: string;
  description: string;
  mediaUrl: string | null;
  muscleGroup: MuscleGroup | null;
  equipment: Equipment | null;
  source: ExerciseSource;
  isEditable: boolean;
}

export type ExerciseListSource = "all" | "platform" | "my";

export interface CreateExerciseRequest {
  name: string;
  description: string;
  mediaUrl: string | null;
  muscleGroup: MuscleGroup | null;
  equipment: Equipment | null;
}

export interface UpdateExerciseRequest {
  name: string;
  description: string;
  mediaUrl: string | null;
  muscleGroup: MuscleGroup | null;
  equipment: Equipment | null;
}
