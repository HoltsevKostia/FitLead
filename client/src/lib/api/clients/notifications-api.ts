import type {
  Notification,
  UnreadNotificationCount,
} from "@/entities/notification/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const notificationsApi = {
  getNotifications(limit = 10): Promise<Notification[]> {
    const params = new URLSearchParams();
    params.set("limit", limit.toString());

    return apiRequest<Notification[]>(`/api/notifications?${params.toString()}`);
  },

  getUnreadCount(): Promise<UnreadNotificationCount> {
    return apiRequest<UnreadNotificationCount>("/api/notifications/unread-count");
  },

  markRead(notificationId: string): Promise<void> {
    return apiRequest<void>(
      `/api/notifications/${encodeURIComponent(notificationId)}/read`,
      {
        method: "POST",
        responseType: "void",
      },
    );
  },

  markAllRead(): Promise<void> {
    return apiRequest<void>("/api/notifications/read-all", {
      method: "POST",
      responseType: "void",
    });
  },
};
