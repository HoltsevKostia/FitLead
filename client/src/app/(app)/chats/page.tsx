import type { ChatListItem } from "@/entities/chat/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getChats } from "@/features/chats/server/get-chats";
import { ChatList } from "@/features/chats/ui/chat-list";

export default async function ChatsPage() {
  const currentUser = await getCurrentUser();

  if (!currentUser) {
    return null;
  }

  let chats: ChatListItem[] = [];
  let loadError: string | null = null;

  try {
    chats = await getChats();
  } catch {
    loadError = "Не вдалося завантажити список чатів. Спробуйте оновити сторінку.";
  }

  return <ChatList chats={chats} currentUser={currentUser} loadError={loadError} />;
}
