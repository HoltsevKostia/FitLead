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
