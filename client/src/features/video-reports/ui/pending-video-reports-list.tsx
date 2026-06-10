import Link from "next/link";

import type { PendingTrainerVideoReport } from "@/entities/video-report/model/types";
import { PlainText } from "@/shared/ui/plain-text";

interface PendingVideoReportsListProps {
  reports: PendingTrainerVideoReport[];
}

function formatSubmittedAt(value: string): string {
  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function pluralize(
  value: number,
  one: string,
  few: string,
  many: string,
): string {
  const lastTwoDigits = value % 100;
  const lastDigit = value % 10;

  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    return many;
  }

  if (lastDigit === 1) {
    return one;
  }

  if (lastDigit >= 2 && lastDigit <= 4) {
    return few;
  }

  return many;
}

function formatWaitingTime(createdAtUtc: string): string {
  const elapsedMilliseconds = Math.max(
    0,
    Date.now() - new Date(createdAtUtc).getTime(),
  );
  const elapsedMinutes = Math.floor(elapsedMilliseconds / 60_000);

  if (elapsedMinutes < 60) {
    const minutes = Math.max(1, elapsedMinutes);
    return `${minutes} ${pluralize(minutes, "хвилину", "хвилини", "хвилин")}`;
  }

  const elapsedHours = Math.floor(elapsedMinutes / 60);
  if (elapsedHours < 24) {
    return `${elapsedHours} ${pluralize(elapsedHours, "годину", "години", "годин")}`;
  }

  const elapsedDays = Math.floor(elapsedHours / 24);
  return `${elapsedDays} ${pluralize(elapsedDays, "день", "дні", "днів")}`;
}

function PendingVideoReportCard({
  report,
}: {
  report: PendingTrainerVideoReport;
}) {
  return (
    <article className="rounded-2xl border border-border bg-white p-4 sm:p-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2 text-xs font-semibold">
            <span className="rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-amber-800">
              Очікує відгуку
            </span>
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-muted">
              {report.mediaCount} медіа
            </span>
          </div>

          <div>
            <h2 className="break-words text-lg font-semibold text-foreground">
              {report.title}
            </h2>
            <p className="mt-1 break-words text-sm font-medium text-foreground">
              {report.clientName}
            </p>
          </div>

          <div className="flex flex-col gap-1 text-sm text-muted md:flex-row md:flex-wrap md:gap-x-4">
            <span>Надіслано: {formatSubmittedAt(report.createdAtUtc)}</span>
            <span>Очікує: {formatWaitingTime(report.createdAtUtc)}</span>
          </div>
        </div>

        <Link
          href={`/chats/${report.chatId}/reports/${report.reportId}`}
          className="inline-flex w-full shrink-0 justify-center rounded-full bg-accent px-4 py-2 text-sm font-medium text-white transition hover:bg-accent-strong sm:w-auto"
        >
          Відкрити
        </Link>
      </div>

      {report.description ? (
        <div className="mt-4 rounded-xl border border-border bg-surface px-4 py-3">
          <PlainText className="line-clamp-4 break-words text-sm leading-6 text-muted">
            {report.description}
          </PlainText>
        </div>
      ) : null}
    </article>
  );
}

export function PendingVideoReportsList({
  reports,
}: PendingVideoReportsListProps) {
  if (reports.length === 0) {
    return (
      <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
        <p className="text-lg font-medium text-foreground">
          Звітів, що очікують відгуку, немає.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {reports.map((report) => (
        <PendingVideoReportCard key={report.reportId} report={report} />
      ))}
    </div>
  );
}
