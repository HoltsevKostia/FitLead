"use client";

import { useLayoutEffect, useRef, useState } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { chatsApi } from "@/lib/api/clients/chats-api";
import { FormAlert } from "@/shared/forms/form-alert";

const HISTORY_PAGE_SIZE = 50;
const messageTimeFormatter = new Intl.DateTimeFormat("uk-UA", {
  hour: "2-digit",
  minute: "2-digit",
});

interface ChatHistoryProps {
  chatId: string;
  currentUser: CurrentUser;
  initialHasMore: boolean;
  initialMessages: ChatMessage[];
}

function formatMessageTime(value: string): string {
  return messageTimeFormatter.format(new Date(value));
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
    <div className={`flex min-w-0 ${isOwn ? "justify-end" : "justify-start"}`}>
      <article
        className={`min-w-0 max-w-[min(88%,42rem)] rounded-2xl px-4 py-3 sm:max-w-[min(78%,42rem)] ${
          isOwn
            ? "rounded-br-md bg-accent text-white"
            : "rounded-bl-md border border-border bg-surface text-foreground"
        }`}
      >
        {!isOwn ? (
          <p className="mb-1 text-xs font-medium text-muted">{message.senderName}</p>
        ) : null}
        <p className="whitespace-pre-wrap break-words text-sm leading-6">
          {message.text ?? ""}
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

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося завантажити повідомлення.";
}

export function ChatHistory({
  chatId,
  currentUser,
  initialHasMore,
  initialMessages,
}: ChatHistoryProps) {
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const didInitialScrollRef = useRef(false);
  const scrollHeightBeforePrependRef = useRef<number | null>(null);
  const [messages, setMessages] = useState(initialMessages);
  const [hasMore, setHasMore] = useState(initialHasMore);
  const [isLoadingOlder, setIsLoadingOlder] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function scrollToLatest() {
    scrollContainerRef.current?.scrollTo({
      top: scrollContainerRef.current.scrollHeight,
      behavior: "smooth",
    });
  }

  useLayoutEffect(() => {
    const scrollHeightBeforePrepend = scrollHeightBeforePrependRef.current;
    const scrollContainer = scrollContainerRef.current;

    if (!didInitialScrollRef.current && scrollContainer) {
      scrollContainer.scrollTop = scrollContainer.scrollHeight;
      didInitialScrollRef.current = true;
      return;
    }

    if (scrollHeightBeforePrepend === null || !scrollContainer) {
      return;
    }

    scrollContainer.scrollTop +=
      scrollContainer.scrollHeight - scrollHeightBeforePrepend;
    scrollHeightBeforePrependRef.current = null;
  }, [messages.length]);

  async function loadOlderMessages() {
    const oldestMessage = messages[0];

    if (!oldestMessage || isLoadingOlder) {
      return;
    }

    scrollHeightBeforePrependRef.current =
      scrollContainerRef.current?.scrollHeight ?? null;
    setIsLoadingOlder(true);
    setError(null);

    try {
      const history = await chatsApi.getMessages(chatId, {
        limit: HISTORY_PAGE_SIZE,
        beforeCreatedAtUtc: oldestMessage.createdAtUtc,
      });

      setMessages((currentMessages) => {
        const existingIds = new Set(currentMessages.map((message) => message.id));
        const olderMessages = history.items.filter(
          (message) => !existingIds.has(message.id),
        );

        return [...olderMessages, ...currentMessages];
      });
      setHasMore(history.hasMore);
    } catch (caughtError) {
      setError(getErrorMessage(caughtError));
    } finally {
      setIsLoadingOlder(false);
    }
  }

  if (messages.length === 0) {
    return (
      <div
        ref={scrollContainerRef}
        className="flex h-full min-h-64 items-center justify-center overflow-y-auto px-3 py-5 text-center sm:px-5 sm:py-6"
      >
        <p className="text-sm text-muted">Повідомлень поки немає</p>
      </div>
    );
  }

  return (
    <div
      ref={scrollContainerRef}
      className="h-full min-w-0 space-y-4 overflow-y-auto px-3 py-5 sm:px-5 sm:py-6"
    >
      {hasMore ? (
        <div className="flex justify-center">
          <button
            type="button"
            onClick={() => {
              void loadOlderMessages();
            }}
            disabled={isLoadingOlder}
            className="rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isLoadingOlder ? "Завантаження..." : "Завантажити старіші"}
          </button>
        </div>
      ) : null}

      <FormAlert message={error} />

      <div className="min-w-0 space-y-3">
        {messages.map((message) => (
          <MessageBubble
            key={message.id}
            currentUser={currentUser}
            message={message}
          />
        ))}
      </div>

      <div className="sticky bottom-0 flex justify-end pb-1 pt-2">
        <button
          type="button"
          onClick={scrollToLatest}
          className="rounded-full border border-border bg-white px-4 py-2 text-sm font-medium text-foreground shadow-sm transition hover:bg-surface-strong"
        >
          До останніх
        </button>
      </div>
    </div>
  );
}
