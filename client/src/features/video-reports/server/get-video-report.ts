import type { VideoReportDetails } from "@/entities/video-report/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getVideoReport(
  chatId: string,
  reportId: string,
): Promise<VideoReportDetails> {
  return serverApiRequest<VideoReportDetails>(
    `/api/chats/${encodeURIComponent(chatId)}/video-reports/${encodeURIComponent(reportId)}`,
  );
}
