/* eslint-disable @next/next/no-img-element */

import Link from "next/link";

import type {
  VideoReportDetails,
  VideoReportMedia,
} from "@/entities/video-report/model/types";

interface VideoReportDetailViewProps {
  report: VideoReportDetails;
}

const dateTimeFormatter = new Intl.DateTimeFormat("uk-UA", {
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

function formatDateTime(value: string): string {
  return dateTimeFormatter.format(new Date(value));
}

function getStatusLabel(status: string): string {
  const labels: Record<string, string> = {
    Submitted: "Очікує відгуку",
    Reviewed: "Переглянуто",
  };

  return labels[status] ?? status;
}

function formatFileSize(sizeBytes: number): string {
  if (sizeBytes < 1024 * 1024) {
    return `${Math.ceil(sizeBytes / 1024)} KB`;
  }

  return `${(sizeBytes / 1024 / 1024).toFixed(1)} MB`;
}

function VideoReportMediaItem({ media }: { media: VideoReportMedia }) {
  return (
    <article className="overflow-hidden rounded-2xl border border-border bg-white">
      {media.kind === "Image" ? (
        <a href={media.deliveryUrl} target="_blank" rel="noreferrer">
          <img
            src={media.deliveryUrl}
            alt={media.fileName ?? ""}
            className="max-h-[34rem] w-full object-contain"
            referrerPolicy="no-referrer"
          />
        </a>
      ) : (
        <video
          controls
          className="max-h-[34rem] w-full bg-black"
          src={media.deliveryUrl}
        >
          <a href={media.deliveryUrl} target="_blank" rel="noreferrer">
            Відкрити відео
          </a>
        </video>
      )}

      <div className="space-y-1 border-t border-border px-4 py-3">
        <p className="break-words text-sm font-medium text-foreground">
          {media.fileName ?? "Медіафайл"}
        </p>
        <p className="text-xs text-muted">
          {media.kind === "Image" ? "Фото" : "Відео"} · {formatFileSize(media.sizeBytes)}
          {media.durationSeconds ? ` · ${media.durationSeconds} с` : ""}
        </p>
      </div>
    </article>
  );
}

export function VideoReportDetailView({ report }: VideoReportDetailViewProps) {
  return (
    <section className="mx-auto flex w-full max-w-4xl flex-col gap-6">
      <header className="space-y-4">
        <Link
          href={`/chats/${report.chatId}`}
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Назад до чату
        </Link>

        <div className="space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            <span className="rounded-full border border-border bg-white px-3 py-1 text-xs font-semibold text-muted">
              Відео-звіт
            </span>
            <span className="rounded-full border border-border bg-white px-3 py-1 text-xs font-semibold text-muted">
              {getStatusLabel(report.status)}
            </span>
          </div>
          <h1 className="break-words text-3xl font-semibold text-foreground">
            {report.title}
          </h1>
          <p className="text-sm text-muted">
            Створено {formatDateTime(report.createdAtUtc)}
          </p>
        </div>
      </header>

      {report.description ? (
        <section className="rounded-2xl border border-border bg-white px-5 py-5">
          <h2 className="text-base font-semibold text-foreground">Опис</h2>
          <p className="mt-3 whitespace-pre-wrap break-words text-sm leading-6 text-muted">
            {report.description}
          </p>
        </section>
      ) : null}

      <section className="space-y-3">
        <h2 className="text-base font-semibold text-foreground">Медіа</h2>
        <div className="grid gap-4">
          {report.media.map((media) => (
            <VideoReportMediaItem key={media.id} media={media} />
          ))}
        </div>
      </section>

      {report.trainerFeedbackText ? (
        <section className="rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-5">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-base font-semibold text-emerald-950">
              Відгук тренера
            </h2>
            {report.reviewedAtUtc ? (
              <span className="text-xs font-medium text-emerald-800">
                {formatDateTime(report.reviewedAtUtc)}
              </span>
            ) : null}
          </div>
          <p className="mt-3 whitespace-pre-wrap break-words text-sm leading-6 text-emerald-950">
            {report.trainerFeedbackText}
          </p>
        </section>
      ) : null}
    </section>
  );
}
