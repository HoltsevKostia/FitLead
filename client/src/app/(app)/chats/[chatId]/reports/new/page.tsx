import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getChat } from "@/features/chats/server/get-chat";
import { CreateVideoReportShell } from "@/features/video-reports/ui/create-video-report-shell";
import { isApiError } from "@/lib/api/api-error";

interface NewVideoReportPageProps {
  params: Promise<{
    chatId: string;
  }>;
}

async function getVisibleChatOrNotFound(chatId: string) {
  try {
    return await getChat(chatId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    throw error;
  }
}

export default async function NewVideoReportPage({
  params,
}: NewVideoReportPageProps) {
  const currentUser = await getCurrentUser();
  if (!currentUser || currentUser.role !== "Client") {
    notFound();
  }

  const { chatId } = await params;
  const chat = await getVisibleChatOrNotFound(chatId);

  return <CreateVideoReportShell chat={chat} />;
}
