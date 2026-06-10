import type { TrainerDashboardSummary } from "@/features/trainer-dashboard/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getTrainerDashboardSummary(): Promise<TrainerDashboardSummary> {
  return serverApiRequest<TrainerDashboardSummary>("/api/trainer/dashboard");
}
