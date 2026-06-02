import type { TrainerClientVideoReport } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientVideoReports(
  clientId: string,
): Promise<TrainerClientVideoReport[]> {
  return serverApiRequest<TrainerClientVideoReport[]>(
    `/api/trainer/clients/${clientId}/video-reports`,
  );
}
