"use client";

import Link from "next/link";
import { useState } from "react";

import type { VideoReportDetails, VideoReportMedia } from "@/entities/video-report/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { MediaLightbox } from "@/features/media-assets/ui/media-lightbox";
import { MediaVideo } from "@/features/media-assets/ui/media-video";
import { SubmitVideoReportFeedbackForm } from "@/features/video-reports/ui/submit-video-report-feedback-form";
import { MediaImage } from "@/shared/ui/media-image";

interface VideoReportDetailViewProps {
  currentUser: CurrentUser;
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

function getStatusClassName(status: string): string {
  if (status === "Submitted") {
    return "border-sky-200 bg-sky-50 text-sky-800";
  }

  if (status === "Reviewed") {
    return "border-emerald-200 bg-emerald-50 text-emerald-800";
  }

  return "border-border bg-white text-muted";
}

function formatFileSize(sizeBytes: number): string {
  if (sizeBytes < 1024 * 1024) {
    return `${Math.ceil(sizeBytes / 1024)} KB`;
  }

  return `${(sizeBytes / 1024 / 1024).toFixed(1)} MB`;
}

function getMediaKindLabel(media: VideoReportMedia): string {
  return media.kind === "Image" ? "Фото" : "Відео";
}

function VideoReportMediaItem({
  media,
  reportTitle,
  onOpen,
}: {
  media: VideoReportMedia;
  reportTitle: string;
  onOpen: (media: VideoReportMedia) => void;
}) {
  return (
    <article className="overflow-hidden rounded-2xl border border-border bg-white">
      {media.kind === "Image" ? (
        <button
          type="button"
          onClick={() => onOpen(media)}
          className="block w-full"
          aria-label="Відкрити медіа звіту"
        >
          <MediaImage
            src={media.deliveryUrl}
            alt={`Медіа відеозвіту: ${reportTitle}`}
            aspectRatio="video"
            objectFit="contain"
            className="w-full"
            sizes="(max-width: 1024px) 100vw, 896px"
            imageClassName="!max-h-none"
          />
        </button>
      ) : (
        <div className="space-y-3 bg-black pb-3">
          <MediaVideo
            src={media.deliveryUrl}
            objectFit="contain"
          />
          <div className="px-4">
            <button
              type="button"
              onClick={() => onOpen(media)}
              className="rounded-full border border-white/20 px-4 py-2 text-sm font-medium text-white transition hover:bg-white/10"
            >
              Відкрити у перегляді
            </button>
          </div>
        </div>
      )}

      <div className="space-y-1 border-t border-border px-4 py-3">
        <p className="break-words text-sm font-medium text-foreground">
          {media.fileName ?? "Медіафайл"}
        </p>
        <p className="text-xs text-muted">
          {getMediaKindLabel(media)} · {formatFileSize(media.sizeBytes)}
          {media.durationSeconds ? ` · ${media.durationSeconds} с` : ""}
        </p>
      </div>
    </article>
  );
}

export function VideoReportDetailView({
  currentUser,
  report,
}: VideoReportDetailViewProps) {
  const [openedMedia, setOpenedMedia] = useState<VideoReportMedia | null>(null);
  const canSubmitFeedback =
    currentUser.role === "Trainer" && report.status === "Submitted";

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
            <span
              className={`rounded-full border px-3 py-1 text-xs font-semibold ${getStatusClassName(report.status)}`}
            >
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
            <VideoReportMediaItem
              key={media.id}
              media={media}
              reportTitle={report.title}
              onOpen={setOpenedMedia}
            />
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

      {canSubmitFeedback ? (
        <SubmitVideoReportFeedbackForm
          chatId={report.chatId}
          reportId={report.id}
        />
      ) : null}

      {openedMedia ? (
        <MediaLightbox
          asset={openedMedia}
          title={openedMedia.fileName ?? getMediaKindLabel(openedMedia)}
          subtitle={getMediaKindLabel(openedMedia)}
          onClose={() => setOpenedMedia(null)}
        />
      ) : null}
    </section>
  );
}
