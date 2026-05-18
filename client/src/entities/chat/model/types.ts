export interface ChatListItem {
  id: string;
  trainerId: string;
  trainerName: string;
  clientId: string;
  clientName: string;
  lastMessageAtUtc: string | null;
}

export interface Chat {
  id: string;
  trainerId: string;
  clientId: string;
  createdAtUtc: string;
  lastMessageAtUtc: string | null;
}

export interface ChatDetails {
  id: string;
  trainerId: string;
  trainerName: string;
  clientId: string;
  clientName: string;
  createdAtUtc: string;
  lastMessageAtUtc: string | null;
}

export interface ChatMessage {
  id: string;
  chatId: string;
  senderId: string;
  senderName: string;
  type: string;
  text: string | null;
  videoReport: {
    id: string;
    title: string;
    description: string | null;
    status: string;
    mediaCount: number;
  } | null;
  createdAtUtc: string;
}

export interface ChatMessageHistory {
  items: ChatMessage[];
  hasMore: boolean;
}
