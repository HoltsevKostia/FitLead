import type { Exercise, ExerciseListSource } from "@/entities/exercise/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const exercisesApi = {
  getExercises(source: ExerciseListSource = "all"): Promise<Exercise[]> {
    return apiRequest<Exercise[]>(`/api/exercises?source=${source}`);
  },
};
