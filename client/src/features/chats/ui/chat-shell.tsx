import Link from "next/link";

import type { ChatDetails, ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { ChatHistory } from "@/features/chats/ui/chat-history";
import { MessageComposer } from "@/features/chats/ui/message-composer";

interface ChatShellProps {
  chat: ChatDetails;
  currentUser: CurrentUser;
  hasMoreMessages: boolean;
  messages: ChatMessage[];
}

function getCompanionName(chat: ChatDetails, currentUser: CurrentUser): string {
  return currentUser.role === "Trainer" ? chat.clientName : chat.trainerName;
}

function getHistoryKey(messages: ChatMessage[]): string {
  const firstMessageId = messages[0]?.id ?? "empty";
  const lastMessageId = messages.at(-1)?.id ?? "empty";

  return `${messages.length}:${firstMessageId}:${lastMessageId}`;
}

export function ChatShell({
  chat,
  currentUser,
  hasMoreMessages,
  messages,
}: ChatShellProps) {
  return (
    <section className="flex h-[calc(100dvh-8rem)] min-h-[32rem] min-w-0 flex-col overflow-hidden rounded-2xl border border-border bg-white md:h-[calc(100vh-9rem)]">
      <header className="min-w-0 border-b border-border px-4 py-4 sm:px-5">
        <Link
          href="/chats"
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Назад до чатів
        </Link>
        <h1 className="mt-3 break-words text-xl font-semibold text-foreground sm:text-2xl">
          {getCompanionName(chat, currentUser)}
        </h1>
      </header>

      <div className="min-h-0 min-w-0 flex-1">
        <ChatHistory
          key={getHistoryKey(messages)}
          chatId={chat.id}
          currentUser={currentUser}
          initialHasMore={hasMoreMessages}
          initialMessages={messages}
        />
      </div>

      <footer className="min-w-0 border-t border-border px-3 py-3 sm:px-5 sm:py-4">
        <MessageComposer chatId={chat.id} />
      </footer>
    </section>
  );
}
