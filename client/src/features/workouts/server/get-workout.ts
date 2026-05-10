import type { WorkoutDetails } from "@/entities/workout/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getWorkout(workoutId: string): Promise<WorkoutDetails> {
  return serverApiRequest<WorkoutDetails>(`/api/workouts/${workoutId}`);
}
