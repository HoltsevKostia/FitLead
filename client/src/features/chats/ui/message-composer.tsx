"use client";

import { type KeyboardEvent, useState } from "react";

import type { ChatMessage } from "@/entities/chat/model/types";
import { chatsApi } from "@/lib/api/clients/chats-api";
import { FormAlert } from "@/shared/forms/form-alert";

const MAX_MESSAGE_LENGTH = 4000;

interface MessageComposerProps {
  chatId: string;
  onMessageSent: (message: ChatMessage) => void;
}

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося надіслати повідомлення.";
}

export function MessageComposer({ chatId, onMessageSent }: MessageComposerProps) {
  const [text, setText] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submitMessage() {
    const trimmedText = text.trim();

    if (!trimmedText) {
      setError("Введіть повідомлення.");
      return;
    }

    if (trimmedText.length > MAX_MESSAGE_LENGTH) {
      setError(`Повідомлення не може бути довшим за ${MAX_MESSAGE_LENGTH} символів.`);
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const message = await chatsApi.sendTextMessage(chatId, { text: trimmedText });
      onMessageSent(message);
      setText("");
    } catch (caughtError) {
      setError(getErrorMessage(caughtError));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleSubmit(event: SubmitEvent) {
    event.preventDefault();
    await submitMessage();
  }

  function handleKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      void submitMessage();
    }
  }

  return (
    <form
      className="space-y-2"
      onSubmit={(event) => {
        void handleSubmit(event.nativeEvent);
      }}
    >
      <div className="flex min-w-0 items-end gap-2 sm:gap-3">
        <textarea
          value={text}
          onChange={(event) => {
            setText(event.target.value);
            if (error) {
              setError(null);
            }
          }}
          onKeyDown={handleKeyDown}
          disabled={isSubmitting}
          rows={1}
          maxLength={MAX_MESSAGE_LENGTH + 1}
          placeholder="Повідомлення..."
          className="max-h-32 min-h-12 min-w-0 flex-1 resize-none rounded-2xl border border-border bg-surface px-4 py-3 text-sm text-foreground outline-none transition focus:border-accent disabled:cursor-not-allowed disabled:opacity-70"
        />
        <button
          type="submit"
          disabled={isSubmitting || text.trim().length === 0}
          className="h-12 shrink-0 rounded-full bg-accent px-4 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70 sm:px-5"
        >
          {isSubmitting ? "Надсилання..." : "Надіслати"}
        </button>
      </div>
      <FormAlert message={error} />
    </form>
  );
}
