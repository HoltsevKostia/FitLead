import type { TrainingProgramWorkout } from "@/entities/training-program/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainingProgramWorkouts(
  programId: string,
): Promise<TrainingProgramWorkout[]> {
  return serverApiRequest<TrainingProgramWorkout[]>(
    `/api/training-programs/${programId}/workouts`,
  );
}
