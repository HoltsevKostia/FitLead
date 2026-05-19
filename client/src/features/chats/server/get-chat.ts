import type { ChatDetails } from "@/entities/chat/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getChat(chatId: string): Promise<ChatDetails> {
  return serverApiRequest<ChatDetails>(`/api/chats/${encodeURIComponent(chatId)}`);
}
