"use client";

import { useState } from "react";

interface CopyInviteLinkButtonProps {
  inviteUrl: string;
}

export function CopyInviteLinkButton({ inviteUrl }: CopyInviteLinkButtonProps) {
  const [status, setStatus] = useState<"idle" | "success" | "error">("idle");

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(inviteUrl);
      setStatus("success");
    } catch {
      setStatus("error");
    }
  }

  return (
    <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
      <button
        type="button"
        onClick={handleCopy}
        className="rounded-full border border-border px-4 py-2 text-sm font-medium transition hover:bg-surface-strong"
      >
        {status === "success" ? "Скопійовано" : "Скопіювати посилання"}
      </button>
      {status === "error" ? (
        <p className="text-sm text-muted">
          Не вдалося скопіювати. Скопіюй посилання вручну.
        </p>
      ) : null}
    </div>
  );
}
