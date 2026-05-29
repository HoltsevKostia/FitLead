import type { TrainerClientWorkoutLog } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientWorkoutLogs(
  clientId: string,
): Promise<TrainerClientWorkoutLog[]> {
  return serverApiRequest<TrainerClientWorkoutLog[]>(
    `/api/trainer/clients/${clientId}/workout-logs`,
  );
}
