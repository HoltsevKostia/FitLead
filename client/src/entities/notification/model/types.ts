export interface Notification {
  id: string;
  recipientUserId: string;
  type: string;
  title: string;
  body: string | null;
  linkUrl: string;
  isRead: boolean;
  createdAtUtc: string;
  readAtUtc: string | null;
}

export interface UnreadNotificationCount {
  count: number;
}
