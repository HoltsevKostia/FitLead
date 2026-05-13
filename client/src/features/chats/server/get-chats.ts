import type { ChatListItem } from "@/entities/chat/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getChats(): Promise<ChatListItem[]> {
  return serverApiRequest<ChatListItem[]>("/api/chats");
}
