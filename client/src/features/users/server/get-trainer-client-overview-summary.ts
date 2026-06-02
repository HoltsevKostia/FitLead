import type { TrainerClientOverviewSummary } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientOverviewSummary(
  clientId: string,
): Promise<TrainerClientOverviewSummary> {
  return serverApiRequest<TrainerClientOverviewSummary>(
    `/api/trainer/clients/${clientId}/overview`,
  );
}
