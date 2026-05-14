import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getChat } from "@/features/chats/server/get-chat";
import { ChatShell } from "@/features/chats/ui/chat-shell";
import { isApiError } from "@/lib/api/api-error";

interface ChatDetailsPageProps {
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

export default async function ChatDetailsPage({ params }: ChatDetailsPageProps) {
  const currentUser = await getCurrentUser();
  if (!currentUser) {
    notFound();
  }

  const { chatId } = await params;
  const chat = await getVisibleChatOrNotFound(chatId);

  return <ChatShell chat={chat} currentUser={currentUser} />;
}
