import Link from "next/link";

import type { ChatDetails, ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { MessageComposer } from "@/features/chats/ui/message-composer";

interface ChatShellProps {
  chat: ChatDetails;
  currentUser: CurrentUser;
  messages: ChatMessage[];
}

function getCompanionName(chat: ChatDetails, currentUser: CurrentUser): string {
  return currentUser.role === "Trainer" ? chat.clientName : chat.trainerName;
}

function formatMessageTime(value: string): string {
  return new Intl.DateTimeFormat("uk-UA", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function MessageBubble({
  currentUser,
  message,
}: {
  currentUser: CurrentUser;
  message: ChatMessage;
}) {
  const isOwn = message.senderId === currentUser.id;

  return (
    <div className={`flex ${isOwn ? "justify-end" : "justify-start"}`}>
      <article
        className={`max-w-[min(78%,42rem)] rounded-2xl px-4 py-3 ${
          isOwn
            ? "rounded-br-md bg-accent text-white"
            : "rounded-bl-md border border-border bg-surface text-foreground"
        }`}
      >
        {!isOwn ? (
          <p className="mb-1 text-xs font-medium text-muted">{message.senderName}</p>
        ) : null}
        <p className="whitespace-pre-wrap break-words text-sm leading-6">
          {message.text}
        </p>
        <p
          className={`mt-2 text-right text-xs ${
            isOwn ? "text-white/75" : "text-muted"
          }`}
        >
          {formatMessageTime(message.createdAtUtc)}
        </p>
      </article>
    </div>
  );
}

export function ChatShell({ chat, currentUser, messages }: ChatShellProps) {
  return (
    <section className="flex min-h-[calc(100vh-9rem)] flex-col overflow-hidden rounded-2xl border border-border bg-white">
      <header className="border-b border-border px-5 py-4">
        <Link
          href="/chats"
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Назад до чатів
        </Link>
        <h1 className="mt-3 text-2xl font-semibold text-foreground">
          {getCompanionName(chat, currentUser)}
        </h1>
      </header>

      <div className="flex-1 overflow-y-auto px-5 py-6">
        {messages.length === 0 ? (
          <div className="flex min-h-64 items-center justify-center text-center">
            <p className="text-sm text-muted">Повідомлень поки немає</p>
          </div>
        ) : (
          <div className="space-y-3">
            {messages.map((message) => (
              <MessageBubble
                key={message.id}
                currentUser={currentUser}
                message={message}
              />
            ))}
          </div>
        )}
      </div>

      <footer className="border-t border-border px-5 py-4">
        <MessageComposer chatId={chat.id} />
      </footer>
    </section>
  );
}
