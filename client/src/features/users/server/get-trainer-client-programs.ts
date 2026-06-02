import type { TrainerClientProgram } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientPrograms(
  clientId: string,
): Promise<TrainerClientProgram[]> {
  return serverApiRequest<TrainerClientProgram[]>(
    `/api/trainer/clients/${clientId}/programs`,
  );
}
