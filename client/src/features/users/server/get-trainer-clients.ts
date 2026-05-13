import type { TrainerClient } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerClients(): Promise<TrainerClient[]> {
  return serverApiRequest<TrainerClient[]>("/api/users/clients");
}
