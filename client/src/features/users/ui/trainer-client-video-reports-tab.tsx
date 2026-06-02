"use client";

import Link from "next/link";

import type { TrainerClientVideoReport } from "@/entities/user/model/types";
import { formatUkrainianDate } from "@/features/users/ui/trainer-client-date-formatting";
import { PlainText } from "@/shared/ui/plain-text";

interface TrainerClientVideoReportsTabProps {
  reports: TrainerClientVideoReport[] | null;
}

function getStatusLabel(status: string): string {
  if (status === "Submitted") {
    return "Очікує відгуку";
  }

  if (status === "Reviewed") {
    return "Переглянуто";
  }

  return status;
}

function getStatusClassName(status: string): string {
  if (status === "Reviewed") {
    return "border-emerald-200 bg-emerald-50 text-emerald-800";
  }

  if (status === "Submitted") {
    return "border-amber-200 bg-amber-50 text-amber-800";
  }

  return "border-border bg-surface text-muted";
}

function VideoReportCard({ report }: { report: TrainerClientVideoReport }) {
  return (
    <article className="rounded-2xl border border-border bg-white px-5 py-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`rounded-full border px-3 py-1 text-xs font-semibold ${getStatusClassName(
                report.status,
              )}`}
            >
              {getStatusLabel(report.status)}
            </span>
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {report.mediaCount} медіа
            </span>
          </div>

          <h2 className="break-words text-lg font-semibold text-foreground">
            {report.title}
          </h2>
          <p className="text-sm text-muted">
            Створено: {formatUkrainianDate(report.createdAtUtc)}
          </p>
          {report.reviewedAtUtc ? (
            <p className="text-sm text-muted">
              Переглянуто: {formatUkrainianDate(report.reviewedAtUtc)}
            </p>
          ) : null}
        </div>

        <Link
          href={`/chats/${report.chatId}/reports/${report.reportId}`}
          className="w-fit rounded-full bg-accent px-4 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
        >
          Відкрити
        </Link>
      </div>

      {report.description ? (
        <div className="mt-4 rounded-xl border border-border bg-surface px-4 py-3">
          <PlainText className="line-clamp-4 text-sm leading-6 text-muted">
            {report.description}
          </PlainText>
        </div>
      ) : null}
    </article>
  );
}

export function TrainerClientVideoReportsTab({
  reports,
}: TrainerClientVideoReportsTabProps) {
  if (!reports) {
    return null;
  }

  if (reports.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-5 py-6">
        <p className="text-sm text-muted">Відео-звітів ще немає.</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {reports.map((report) => (
        <VideoReportCard key={report.reportId} report={report} />
      ))}
    </div>
  );
}
