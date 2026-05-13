import type { TrainerClient, TrainerClientOverview } from "@/entities/user/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const usersApi = {
  getTrainerClients(): Promise<TrainerClient[]> {
    return apiRequest<TrainerClient[]>("/api/users/clients");
  },

  getTrainerClientsOverview(): Promise<TrainerClientOverview[]> {
    return apiRequest<TrainerClientOverview[]>("/api/trainer/clients");
  },
};
