import type { Exercise } from "@/entities/exercise/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getExercise(exerciseId: string): Promise<Exercise> {
  return serverApiRequest<Exercise>(`/api/exercises/${exerciseId}`);
}
