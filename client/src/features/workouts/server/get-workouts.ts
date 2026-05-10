import type { Workout } from "@/entities/workout/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getWorkouts(): Promise<Workout[]> {
  return serverApiRequest<Workout[]>("/api/workouts");
}
