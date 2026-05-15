import type { ChatDetails, ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { ChatThread } from "@/features/chats/ui/chat-thread";

interface ChatShellProps {
  chat: ChatDetails;
  currentUser: CurrentUser;
  hasMoreMessages: boolean;
  messages: ChatMessage[];
}

function getCompanionName(chat: ChatDetails, currentUser: CurrentUser): string {
  return currentUser.role === "Trainer" ? chat.clientName : chat.trainerName;
}

function getThreadKey(chatId: string, messages: ChatMessage[]): string {
  const firstMessageId = messages[0]?.id ?? "empty";
  const lastMessageId = messages.at(-1)?.id ?? "empty";

  return `${chatId}:${messages.length}:${firstMessageId}:${lastMessageId}`;
}

export function ChatShell({
  chat,
  currentUser,
  hasMoreMessages,
    messages,
}: ChatShellProps) {
  return (
    <section className="flex h-[calc(100dvh-8rem)] min-h-[32rem] min-w-0 flex-col overflow-hidden rounded-2xl border border-border bg-white md:h-[calc(100vh-9rem)]">
      <ChatThread
        key={getThreadKey(chat.id, messages)}
        chatId={chat.id}
        companionName={getCompanionName(chat, currentUser)}
        currentUser={currentUser}
        initialHasMore={hasMoreMessages}
        initialMessages={messages}
      />
    </section>
  );
}
