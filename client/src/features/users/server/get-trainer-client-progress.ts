import type { TrainerClientProgress } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientProgress(
  clientId: string,
): Promise<TrainerClientProgress> {
  return serverApiRequest<TrainerClientProgress>(
    `/api/trainer/clients/${clientId}/progress`,
  );
}
