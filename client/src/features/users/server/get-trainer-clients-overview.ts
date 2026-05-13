import type { TrainerClientOverview } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClientsOverview(): Promise<TrainerClientOverview[]> {
  return serverApiRequest<TrainerClientOverview[]>("/api/trainer/clients");
}
