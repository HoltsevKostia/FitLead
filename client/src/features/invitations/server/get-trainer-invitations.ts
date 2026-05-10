import type { TrainerInvitation } from "@/entities/invitation/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainerInvitations(): Promise<TrainerInvitation[]> {
  return serverApiRequest<TrainerInvitation[]>("/api/invitations/trainer");
}
