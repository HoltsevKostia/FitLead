import type { ChatMessageHistory } from "@/entities/chat/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getChatMessages(
  chatId: string,
  limit = 50,
): Promise<ChatMessageHistory> {
  const params = new URLSearchParams({
    limit: limit.toString(),
  });

  return serverApiRequest<ChatMessageHistory>(
    `/api/chats/${encodeURIComponent(chatId)}/messages?${params.toString()}`,
  );
}
