import Link from "next/link";

import type { ChatDetails } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";

interface ChatShellProps {
  chat: ChatDetails;
  currentUser: CurrentUser;
}

function getCompanionName(chat: ChatDetails, currentUser: CurrentUser): string {
  return currentUser.role === "Trainer" ? chat.clientName : chat.trainerName;
}

export function ChatShell({ chat, currentUser }: ChatShellProps) {
  return (
    <section className="flex min-h-[calc(100vh-9rem)] flex-col rounded-2xl border border-border bg-white">
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

      <div className="flex flex-1 items-center justify-center px-5 py-8 text-center">
        <p className="text-sm text-muted">Повідомлень поки немає</p>
      </div>

      <footer className="border-t border-border px-5 py-4">
        <input
          type="text"
          disabled
          placeholder="Повідомлення..."
          className="w-full rounded-full border border-border bg-surface px-4 py-3 text-sm text-muted outline-none disabled:cursor-not-allowed"
        />
      </footer>
    </section>
  );
}
