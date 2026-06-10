import type { ClientDashboard } from "@/features/client-dashboard/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getClientDashboard(): Promise<ClientDashboard> {
  return serverApiRequest<ClientDashboard>("/api/client/dashboard");
}
