import type { ClientTrainer } from "@/entities/user/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getMyTrainer(): Promise<ClientTrainer> {
  return serverApiRequest<ClientTrainer>("/api/users/my-trainer");
}
