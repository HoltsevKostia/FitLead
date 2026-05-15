"use client";

import { useCallback, useState } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { useChatMessages } from "@/features/chats/model/use-chat-messages";
import { useChatRealtime } from "@/features/chats/model/use-chat-realtime";
import { ChatHistory } from "@/features/chats/ui/chat-history";
import { MessageComposer } from "@/features/chats/ui/message-composer";

interface ChatThreadProps {
  chatId: string;
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
  currentUser,
  initialHasMore,
  initialMessages,
}: ChatThreadProps) {
  const [error, setError] = useState<string | null>(null);
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
  });

  return (
    <>
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
