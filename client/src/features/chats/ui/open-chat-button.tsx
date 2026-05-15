"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import { chatsApi } from "@/lib/api/clients/chats-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface OpenChatButtonProps {
  targetId: string;
  targetType: "client" | "trainer";
  label?: string;
}

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося відкрити чат. Спробуйте ще раз.";
}

export function OpenChatButton({
  targetId,
  targetType,
  label = "Відкрити чат",
}: OpenChatButtonProps) {
  const router = useRouter();
  const [isOpening, setIsOpening] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleOpenChat() {
    setIsOpening(true);
    setError(null);

    try {
      const chat =
        targetType === "client"
          ? await chatsApi.getOrCreateWithClient(targetId)
          : await chatsApi.getOrCreateWithTrainer(targetId);

      router.push(`/chats/${chat.id}`);
    } catch (caughtError) {
      setError(getErrorMessage(caughtError));
    } finally {
      setIsOpening(false);
    }
  }

  return (
    <div className="space-y-2">
      <button
        type="button"
        onClick={handleOpenChat}
        disabled={isOpening}
        className="w-fit rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isOpening ? "Відкриваємо..." : label}
      </button>
      <FormAlert message={error} />
    </div>
  );
}
