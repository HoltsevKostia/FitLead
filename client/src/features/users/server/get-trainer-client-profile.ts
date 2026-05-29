import type { TrainerClientProfile } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientProfile(
  clientId: string,
): Promise<TrainerClientProfile> {
  return serverApiRequest<TrainerClientProfile>(
    `/api/trainer/clients/${clientId}/profile`,
  );
}
