import Link from "next/link";

import type { ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { formatMessageTime } from "@/features/chats/ui/message-time";

interface VideoReportMessageCardProps {
  currentUser: CurrentUser;
  message: ChatMessage;
}

function getVideoReportStatusLabel(status: string): string {
  const labels: Record<string, string> = {
    Submitted: "Очікує відгуку",
    Reviewed: "Переглянуто",
  };

  return labels[status] ?? status;
}

export function VideoReportMessageCard({
  currentUser,
  message,
}: VideoReportMessageCardProps) {
  const isOwn = message.senderId === currentUser.id;
  const report = message.videoReport;

  return (
    <div className={`flex min-w-0 ${isOwn ? "justify-end" : "justify-start"}`}>
      <article
        className={`min-w-0 max-w-[min(92%,30rem)] rounded-2xl border px-4 py-4 sm:max-w-[min(82%,32rem)] ${
          isOwn
            ? "rounded-br-md border-accent/30 bg-accent/10"
            : "rounded-bl-md border-border bg-surface"
        }`}
      >
        {!isOwn ? (
          <p className="mb-2 text-xs font-medium text-muted">{message.senderName}</p>
        ) : null}

        {report ? (
          <>
            <div className="flex min-w-0 flex-wrap items-center gap-2">
              <span className="rounded-full border border-border bg-white px-2.5 py-1 text-xs font-semibold text-muted">
                Відео-звіт
              </span>
              <span className="rounded-full border border-border bg-white px-2.5 py-1 text-xs font-medium text-muted">
                {getVideoReportStatusLabel(report.status)}
              </span>
            </div>

            <h2 className="mt-3 break-words text-base font-semibold text-foreground">
              {report.title}
            </h2>
            {report.description ? (
              <p className="mt-2 line-clamp-3 whitespace-pre-wrap break-words text-sm leading-6 text-muted">
                {report.description}
              </p>
            ) : null}

            <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
              <span className="text-sm text-muted">
                Медіафайлів: {report.mediaCount}
              </span>
              <Link
                href={`/chats/${message.chatId}/reports/${report.id}`}
                className="rounded-full border border-border bg-white px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
              >
                Відкрити
              </Link>
            </div>
          </>
        ) : (
          <p className="text-sm text-muted">Відео-звіт недоступний.</p>
        )}

        <p className="mt-3 text-right text-xs text-muted">
          {formatMessageTime(message.createdAtUtc)}
        </p>
      </article>
    </div>
  );
}
