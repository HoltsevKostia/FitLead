"use client";

import { useEffect } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import { createChatConnection } from "@/lib/realtime/chat-connection";

interface UseChatRealtimeOptions {
  chatId: string;
  onError: (message: string) => void;
  onMessageCreated: (message: ChatMessage) => void;
}

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося підключитися до чату.";
}

export function useChatRealtime({
  chatId,
  onError,
  onMessageCreated,
}: UseChatRealtimeOptions) {
  useEffect(() => {
    const connection = createChatConnection();
    let isDisposed = false;

    function handleMessageCreated(message: ChatMessage) {
      if (message.chatId === chatId) {
        onMessageCreated(message);
      }
    }

    function handleReconnected() {
      void connection.invoke("JoinChat", chatId).catch((caughtError) => {
        if (!isDisposed) {
          onError(getErrorMessage(caughtError));
        }
      });
    }

    async function startConnection() {
      connection.on("MessageCreated", handleMessageCreated);
      connection.onreconnected(handleReconnected);

      try {
        await connection.start();

        if (!isDisposed) {
          await connection.invoke("JoinChat", chatId);
        }
      } catch (caughtError) {
        if (!isDisposed) {
          onError(getErrorMessage(caughtError));
        }
      }
    }

    const startPromise = startConnection();

    return () => {
      isDisposed = true;
      connection.off("MessageCreated", handleMessageCreated);
      void startPromise.finally(() => connection.stop());
    };
  }, [chatId, onError, onMessageCreated]);
}
