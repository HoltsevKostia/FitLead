"use client";

import { useLayoutEffect, useRef, useState } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { FormAlert } from "@/shared/forms/form-alert";

const NEAR_BOTTOM_THRESHOLD_PX = 96;
const messageTimeFormatter = new Intl.DateTimeFormat("uk-UA", {
  hour: "2-digit",
  minute: "2-digit",
});

interface ChatHistoryProps {
  currentUser: CurrentUser;
  error: string | null;
  hasMore: boolean;
  isLoadingOlder: boolean;
  messages: ChatMessage[];
  onLoadOlder: () => Promise<boolean>;
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

function isNearBottom(scrollContainer: HTMLDivElement | null): boolean {
  if (!scrollContainer) {
    return true;
  }

  const distanceFromBottom =
    scrollContainer.scrollHeight -
    scrollContainer.scrollTop -
    scrollContainer.clientHeight;

  return distanceFromBottom <= NEAR_BOTTOM_THRESHOLD_PX;
}

export function ChatHistory({
  currentUser,
  error,
  hasMore,
  isLoadingOlder,
  messages,
  onLoadOlder,
}: ChatHistoryProps) {
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const didInitialScrollRef = useRef(false);
  const previousMessagesLengthRef = useRef(messages.length);
  const wasAtBottomBeforeAppendRef = useRef(true);
  const scrollHeightBeforePrependRef = useRef<number | null>(null);
  const [isAtBottom, setIsAtBottom] = useState(true);
  const [hasUnreadLiveMessages, setHasUnreadLiveMessages] = useState(false);

  function scrollToLatest() {
    scrollContainerRef.current?.scrollTo({
      top: scrollContainerRef.current.scrollHeight,
      behavior: "smooth",
    });
    setIsAtBottom(true);
    setHasUnreadLiveMessages(false);
  }

  useLayoutEffect(() => {
    const scrollHeightBeforePrepend = scrollHeightBeforePrependRef.current;
    const scrollContainer = scrollContainerRef.current;
    const didAppendMessage = messages.length > previousMessagesLengthRef.current;
    const didPrependMessages = scrollHeightBeforePrepend !== null;

    if (!didInitialScrollRef.current && scrollContainer) {
      scrollContainer.scrollTop = scrollContainer.scrollHeight;
      didInitialScrollRef.current = true;
      previousMessagesLengthRef.current = messages.length;
      setIsAtBottom(true);
      return;
    }

    if (scrollHeightBeforePrepend !== null && scrollContainer) {
      scrollContainer.scrollTop +=
        scrollContainer.scrollHeight - scrollHeightBeforePrepend;
      scrollHeightBeforePrependRef.current = null;
    }

    if (!didAppendMessage || didPrependMessages || !scrollContainer) {
      previousMessagesLengthRef.current = messages.length;
      return;
    }

    if (wasAtBottomBeforeAppendRef.current) {
      scrollContainer.scrollTop = scrollContainer.scrollHeight;
      setIsAtBottom(true);
      setHasUnreadLiveMessages(false);
    } else {
      setHasUnreadLiveMessages(true);
    }

    previousMessagesLengthRef.current = messages.length;
  }, [messages.length]);

  async function loadOlderMessages() {
    if (messages.length === 0 || isLoadingOlder) {
      return;
    }

    scrollHeightBeforePrependRef.current =
      scrollContainerRef.current?.scrollHeight ?? null;

    if (!(await onLoadOlder())) {
      scrollHeightBeforePrependRef.current = null;
    }
  }

  function handleScroll() {
    const atBottom = isNearBottom(scrollContainerRef.current);

    wasAtBottomBeforeAppendRef.current = atBottom;
    setIsAtBottom(atBottom);

    if (atBottom) {
      setHasUnreadLiveMessages(false);
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
      onScroll={handleScroll}
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

      {!isAtBottom || hasUnreadLiveMessages ? (
        <div className="sticky bottom-0 flex justify-end pb-1 pt-2">
          <button
            type="button"
            onClick={scrollToLatest}
            className="rounded-full border border-border bg-white px-4 py-2 text-sm font-medium text-foreground shadow-sm transition hover:bg-surface-strong"
          >
            {hasUnreadLiveMessages ? "Нове повідомлення" : "До останніх"}
          </button>
        </div>
      ) : null}
    </div>
  );
}
