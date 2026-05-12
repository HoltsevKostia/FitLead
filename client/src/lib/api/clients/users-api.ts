import type { TrainerClient } from "@/entities/user/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const usersApi = {
  getTrainerClients(): Promise<TrainerClient[]> {
    return apiRequest<TrainerClient[]>("/api/users/clients");
  },
};
