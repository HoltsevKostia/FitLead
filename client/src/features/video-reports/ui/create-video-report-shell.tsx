import Link from "next/link";

import type { ChatDetails } from "@/entities/chat/model/types";
import { CreateVideoReportForm } from "@/features/video-reports/ui/create-video-report-form";

interface CreateVideoReportShellProps {
  chat: ChatDetails;
}

export function CreateVideoReportShell({ chat }: CreateVideoReportShellProps) {
  return (
    <section className="mx-auto flex w-full max-w-3xl flex-col gap-6">
      <header className="space-y-3">
        <Link
          href={`/chats/${chat.id}`}
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Назад до чату
        </Link>
        <div>
          <p className="text-sm text-muted">Звіт для {chat.trainerName}</p>
          <h1 className="mt-1 text-2xl font-semibold text-foreground">
            Створити відео-звіт
          </h1>
        </div>
      </header>

      <CreateVideoReportForm chat={chat} />
    </section>
  );
}
