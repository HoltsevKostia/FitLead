"use client";

import { useEffect } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import { createChatConnection } from "@/lib/realtime/chat-connection";

export type ChatConnectionStatus =
  | "connecting"
  | "connected"
  | "reconnecting"
  | "disconnected";

interface UseChatRealtimeOptions {
  chatId: string;
  onError: (message: string) => void;
  onMessageCreated: (message: ChatMessage) => void;
  onStatusChange: (status: ChatConnectionStatus) => void;
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
  onStatusChange,
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
      void (async () => {
        try {
          await connection.invoke("JoinChat", chatId);

          if (!isDisposed) {
            onStatusChange("connected");
          }
        } catch (caughtError) {
          if (!isDisposed) {
            onStatusChange("disconnected");
            onError(getErrorMessage(caughtError));
          }
        }
      })();
    }

    function handleReconnecting() {
      if (!isDisposed) {
        onStatusChange("reconnecting");
      }
    }

    function handleClosed() {
      if (!isDisposed) {
        onStatusChange("disconnected");
      }
    }

    async function startConnection() {
      connection.on("MessageCreated", handleMessageCreated);
      connection.onreconnecting(handleReconnecting);
      connection.onreconnected(handleReconnected);
      connection.onclose(handleClosed);

      try {
        onStatusChange("connecting");
        await connection.start();

        if (!isDisposed) {
          await connection.invoke("JoinChat", chatId);
          onStatusChange("connected");
        }
      } catch (caughtError) {
        if (!isDisposed) {
          onStatusChange("disconnected");
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
  }, [chatId, onError, onMessageCreated, onStatusChange]);
}
