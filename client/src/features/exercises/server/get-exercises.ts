import type { Exercise, ExerciseListSource } from "@/entities/exercise/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getExercises(source: ExerciseListSource = "all"): Promise<Exercise[]> {
  return serverApiRequest<Exercise[]>(`/api/exercises?source=${source}`);
}
