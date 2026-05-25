import type { Chat, ChatMessage, ChatMessageHistory } from "@/entities/chat/model/types";
import { apiRequest } from "@/lib/api/http-client";

interface SendTextMessageRequest {
  text: string;
}

interface CreateVideoReportRequest {
  title: string;
  description: string | null;
  mediaAssetIds: string[];
}

interface SubmitVideoReportFeedbackRequest {
  text: string;
}

export const chatsApi = {
  getOrCreateWithClient(clientId: string): Promise<Chat> {
    return apiRequest<Chat>(`/chats/with-client/${encodeURIComponent(clientId)}`, {
      method: "POST",
    });
  },

  getOrCreateWithTrainer(trainerId: string): Promise<Chat> {
    return apiRequest<Chat>(`/chats/with-trainer/${encodeURIComponent(trainerId)}`, {
      method: "POST",
    });
  },

  sendTextMessage(
    chatId: string,
    request: SendTextMessageRequest,
  ): Promise<ChatMessage> {
    return apiRequest<ChatMessage>(
      `/chats/${encodeURIComponent(chatId)}/messages`,
      {
        method: "POST",
        body: request,
      },
    );
  },

  createVideoReport(
    chatId: string,
    request: CreateVideoReportRequest,
  ): Promise<ChatMessage> {
    return apiRequest<ChatMessage>(
      `/chats/${encodeURIComponent(chatId)}/video-reports`,
      {
        method: "POST",
        body: request,
      },
    );
  },

  submitVideoReportFeedback(
    chatId: string,
    reportId: string,
    request: SubmitVideoReportFeedbackRequest,
  ): Promise<void> {
    return apiRequest<void>(
      `/chats/${encodeURIComponent(chatId)}/video-reports/${encodeURIComponent(reportId)}/feedback`,
      {
        method: "POST",
        body: request,
        responseType: "void",
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
      `/chats/${encodeURIComponent(chatId)}/messages${query ? `?${query}` : ""}`,
    );
  },
};
