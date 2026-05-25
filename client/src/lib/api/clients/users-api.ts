import type {
  ClientTrainer,
  TrainerClient,
  TrainerClientOverview,
} from "@/entities/user/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const usersApi = {
  getTrainerClients(): Promise<TrainerClient[]> {
    return apiRequest<TrainerClient[]>("/users/clients");
  },

  getTrainerClientsOverview(): Promise<TrainerClientOverview[]> {
    return apiRequest<TrainerClientOverview[]>("/trainer/clients");
  },

  getMyTrainer(): Promise<ClientTrainer> {
    return apiRequest<ClientTrainer>("/users/my-trainer");
  },
};
