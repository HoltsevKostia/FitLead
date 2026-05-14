import type { Chat, ChatMessage } from "@/entities/chat/model/types";
import { apiRequest } from "@/lib/api/http-client";

interface SendTextMessageRequest {
  text: string;
}

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

  sendTextMessage(
    chatId: string,
    request: SendTextMessageRequest,
  ): Promise<ChatMessage> {
    return apiRequest<ChatMessage>(
      `/api/chats/${encodeURIComponent(chatId)}/messages`,
      {
        method: "POST",
        body: request,
      },
    );
  },
};
