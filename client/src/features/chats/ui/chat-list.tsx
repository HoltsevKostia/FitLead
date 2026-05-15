import Link from "next/link";

import type { ChatListItem } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";

interface ChatListProps {
  chats: ChatListItem[];
  currentUser: CurrentUser;
  loadError?: string | null;
}

function getChatTitle(chat: ChatListItem, currentUser: CurrentUser): string {
  return currentUser.role === "Trainer" ? chat.clientName : chat.trainerName;
}

function getChatSubtitle(chat: ChatListItem, currentUser: CurrentUser): string {
  return currentUser.role === "Trainer" ? "Клієнт" : "Тренер";
}

function formatLastMessageAt(value: string | null): string {
  if (!value) {
    return "Повідомлень ще немає";
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

export function ChatList({ chats, currentUser, loadError }: ChatListProps) {
  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">Чати</h1>
      </div>

      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && chats.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Чатів ще немає.</p>
          {currentUser.role === "Trainer" ? (
            <Link
              href="/clients"
              className="mt-4 inline-flex rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
            >
              Перейти до клієнтів
            </Link>
          ) : null}
        </div>
      ) : null}

      {chats.length > 0 ? (
        <div className="grid gap-4">
          {chats.map((chat) => (
            <article
              key={chat.id}
              className="rounded-2xl border border-border bg-white px-5 py-5"
            >
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="min-w-0">
                  <h2 className="text-xl font-semibold text-foreground">
                    {getChatTitle(chat, currentUser)}
                  </h2>
                  <p className="mt-2 text-sm text-muted">
                    {getChatSubtitle(chat, currentUser)} · {formatLastMessageAt(chat.lastMessageAtUtc)}
                  </p>
                </div>

                <Link
                  href={`/chats/${chat.id}`}
                  className="w-fit rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
                >
                  Відкрити
                </Link>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
