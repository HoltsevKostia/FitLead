"use client";

import Link from "next/link";
import { useCallback, useState } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { useChatMessages } from "@/features/chats/model/use-chat-messages";
import {
  type ChatConnectionStatus,
  useChatRealtime,
} from "@/features/chats/model/use-chat-realtime";
import { ChatHistory } from "@/features/chats/ui/chat-history";
import { MessageComposer } from "@/features/chats/ui/message-composer";

interface ChatThreadProps {
  chatId: string;
  companionName: string;
  currentUser: CurrentUser;
  initialHasMore: boolean;
  initialMessages: ChatMessage[];
}

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося завантажити повідомлення.";
}

export function ChatThread({
  chatId,
  companionName,
  currentUser,
  initialHasMore,
  initialMessages,
}: ChatThreadProps) {
  const [error, setError] = useState<string | null>(null);
  const [connectionStatus, setConnectionStatus] =
    useState<ChatConnectionStatus>("connecting");
  const {
    appendMessage,
    hasMore,
    isLoadingOlder,
    loadOlderMessages,
    messages,
  } = useChatMessages({
    chatId,
    initialHasMore,
    initialMessages,
  });

  const handleRealtimeMessage = useCallback(
    (message: ChatMessage) => {
      appendMessage(message);
    },
    [appendMessage],
  );

  const handleRealtimeError = useCallback((message: string) => {
    setError(message);
  }, []);

  const handleLoadOlder = useCallback(async () => {
    try {
      setError(null);
      return await loadOlderMessages();
    } catch (caughtError) {
      setError(getErrorMessage(caughtError));
      return false;
    }
  }, [loadOlderMessages]);

  useChatRealtime({
    chatId,
    onError: handleRealtimeError,
    onMessageCreated: handleRealtimeMessage,
    onStatusChange: setConnectionStatus,
  });

  return (
    <>
      <header className="min-w-0 border-b border-border px-4 py-4 sm:px-5">
        <div className="flex items-center justify-between gap-3">
          <Link
            href="/chats"
            className="text-sm font-medium text-accent hover:text-accent-strong"
          >
            Назад до чатів
          </Link>
          <ConnectionStatusBadge status={connectionStatus} />
        </div>
        <div className="mt-3 flex min-w-0 flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <h1 className="break-words text-xl font-semibold text-foreground sm:text-2xl">
            {companionName}
          </h1>
          {currentUser.role === "Client" ? (
            <Link
              href={`/chats/${chatId}/reports/new`}
              className="w-fit rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
            >
              Створити відео-звіт
            </Link>
          ) : null}
        </div>
      </header>

      <div className="min-h-0 min-w-0 flex-1">
        <ChatHistory
          currentUser={currentUser}
          error={error}
          hasMore={hasMore}
          isLoadingOlder={isLoadingOlder}
          messages={messages}
          onLoadOlder={handleLoadOlder}
        />
      </div>

      <footer className="min-w-0 border-t border-border px-3 py-3 sm:px-5 sm:py-4">
        <MessageComposer chatId={chatId} onMessageSent={appendMessage} />
      </footer>
    </>
  );
}

function ConnectionStatusBadge({ status }: { status: ChatConnectionStatus }) {
  if (status === "connected") {
    return (
      <span
        aria-label="Real-time з'єднання активне"
        title="Real-time з'єднання активне"
        className="inline-flex h-2.5 w-2.5 shrink-0 rounded-full bg-emerald-500"
      />
    );
  }

  const config = {
    connecting: {
      label: "Підключення...",
      className: "border-border bg-surface text-muted",
    },
    reconnecting: {
      label: "Відновлюємо з'єднання...",
      className: "border-amber-200 bg-amber-50 text-amber-700",
    },
    disconnected: {
      label: "Відсутнє з'єднання",
      className: "border-rose-200 bg-rose-50 text-rose-700",
    },
  }[status];

  return (
    <span
      className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-medium ${config.className}`}
    >
      {config.label}
    </span>
  );
}
