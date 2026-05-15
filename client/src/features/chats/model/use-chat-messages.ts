"use client";

import { useCallback, useState } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import { chatsApi } from "@/lib/api/clients/chats-api";

const HISTORY_PAGE_SIZE = 50;

interface UseChatMessagesOptions {
  chatId: string;
  initialHasMore: boolean;
  initialMessages: ChatMessage[];
}

export function useChatMessages({
  chatId,
  initialHasMore,
  initialMessages,
}: UseChatMessagesOptions) {
  const [messages, setMessages] = useState(initialMessages);
  const [hasMore, setHasMore] = useState(initialHasMore);
  const [isLoadingOlder, setIsLoadingOlder] = useState(false);

  const appendMessage = useCallback((message: ChatMessage): boolean => {
    let didAppend = false;

    setMessages((currentMessages) => {
      if (currentMessages.some((currentMessage) => currentMessage.id === message.id)) {
        return currentMessages;
      }

      didAppend = true;
      return [...currentMessages, message];
    });

    return didAppend;
  }, []);

  const loadOlderMessages = useCallback(async (): Promise<boolean> => {
    const oldestMessage = messages[0];

    if (!oldestMessage || isLoadingOlder) {
      return false;
    }

    setIsLoadingOlder(true);

    try {
      const history = await chatsApi.getMessages(chatId, {
        limit: HISTORY_PAGE_SIZE,
        beforeCreatedAtUtc: oldestMessage.createdAtUtc,
      });
      const knownIds = new Set(messages.map((message) => message.id));
      const didPrepend = history.items.some((message) => !knownIds.has(message.id));

      setMessages((currentMessages) => {
        const existingIds = new Set(currentMessages.map((message) => message.id));
        const olderMessages = history.items.filter(
          (message) => !existingIds.has(message.id),
        );

        if (olderMessages.length === 0) {
          return currentMessages;
        }

        return [...olderMessages, ...currentMessages];
      });
      setHasMore(history.hasMore);

      return didPrepend;
    } finally {
      setIsLoadingOlder(false);
    }
  }, [chatId, isLoadingOlder, messages]);

  return {
    appendMessage,
    hasMore,
    isLoadingOlder,
    loadOlderMessages,
    messages,
  };
}
