import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getVideoReport } from "@/features/video-reports/server/get-video-report";
import { VideoReportDetailView } from "@/features/video-reports/ui/video-report-detail-view";
import { isApiError } from "@/lib/api/api-error";

interface VideoReportDetailsPageProps {
  params: Promise<{
    chatId: string;
    reportId: string;
  }>;
}

async function getVisibleVideoReportOrNotFound(chatId: string, reportId: string) {
  try {
    return await getVideoReport(chatId, reportId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    throw error;
  }
}

export default async function VideoReportDetailsPage({
  params,
}: VideoReportDetailsPageProps) {
  const currentUser = await getCurrentUser();
  if (!currentUser) {
    notFound();
  }

  const { chatId, reportId } = await params;
  const report = await getVisibleVideoReportOrNotFound(chatId, reportId);

  return <VideoReportDetailView report={report} />;
}
