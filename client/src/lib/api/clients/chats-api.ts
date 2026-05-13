import type { Chat } from "@/entities/chat/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const chatsApi = {
  getOrCreateWithClient(clientId: string): Promise<Chat> {
    return apiRequest<Chat>(`/api/chats/with-client/${encodeURIComponent(clientId)}`, {
      method: "POST",
    });
  },

  getOrCreateWithTrainer(trainerId: string): Promise<Chat> {
    return apiRequest<Chat>(`/api/chats/with-trainer/${encodeURIComponent(trainerId)}`, {
      method: "POST",
    });
  },
};
