import type { ClientProfile } from "@/entities/client-profile/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getClientProfile(): Promise<ClientProfile> {
  return serverApiRequest<ClientProfile>("/api/client/profile");
}
