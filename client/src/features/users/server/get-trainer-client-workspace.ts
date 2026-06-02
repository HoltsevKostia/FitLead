import type { TrainerClientWorkspace } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientWorkspace(
  clientId: string,
): Promise<TrainerClientWorkspace> {
  return serverApiRequest<TrainerClientWorkspace>(
    `/api/trainer/clients/${clientId}/workspace`,
  );
}
