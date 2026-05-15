import type { Chat, ChatMessage, ChatMessageHistory } from "@/entities/chat/model/types";
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

  getMessages(
    chatId: string,
    options: { limit?: number; beforeCreatedAtUtc?: string } = {},
  ): Promise<ChatMessageHistory> {
    const params = new URLSearchParams();

    if (options.limit !== undefined) {
      params.set("limit", options.limit.toString());
    }

    if (options.beforeCreatedAtUtc) {
      params.set("beforeCreatedAtUtc", options.beforeCreatedAtUtc);
    }

    const query = params.toString();

    return apiRequest<ChatMessageHistory>(
      `/api/chats/${encodeURIComponent(chatId)}/messages${query ? `?${query}` : ""}`,
    );
  },
};
