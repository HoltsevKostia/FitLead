import type { PendingTrainerVideoReport } from "@/entities/video-report/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getPendingTrainerVideoReports() {
  return serverApiRequest<PendingTrainerVideoReport[]>(
    "/api/trainer/video-reports/pending",
  );
}
