/* eslint-disable @next/next/no-img-element */

"use client";

import { useState } from "react";

import type { BodyMetricEntry } from "@/entities/body-metric/model/types";
import type { ProgressPhoto } from "@/entities/progress-photo/model/types";
import type { TrainerClientProgress } from "@/entities/user/model/types";
import { MediaLightbox } from "@/features/media-assets/ui/media-lightbox";
import { formatUkrainianDateOnly } from "@/features/users/ui/trainer-client-date-formatting";
import { PlainText } from "@/shared/ui/plain-text";

interface TrainerClientProgressTabProps {
  progress: TrainerClientProgress | null;
}

const photoLabelText: Record<ProgressPhoto["label"], string> = {
  Front: "Спереду",
  Side: "Збоку",
  Back: "Ззаду",
  Other: "Інше",
};

function getMetricValues(metric: BodyMetricEntry): string[] {
  return [
    metric.weightKg != null ? `Вага: ${metric.weightKg} кг` : null,
    metric.bodyFatPercent != null ? `Жир: ${metric.bodyFatPercent}%` : null,
    metric.chestCm != null ? `Груди: ${metric.chestCm} см` : null,
    metric.waistCm != null ? `Талія: ${metric.waistCm} см` : null,
    metric.hipsCm != null ? `Стегна: ${metric.hipsCm} см` : null,
    metric.armCm != null ? `Рука: ${metric.armCm} см` : null,
    metric.thighCm != null ? `Нога: ${metric.thighCm} см` : null,
  ].filter((value): value is string => value != null);
}

function getPhotoLabel(label: ProgressPhoto["label"]): string {
  return photoLabelText[label] ?? label;
}

function BodyMetricCard({ metric }: { metric: BodyMetricEntry }) {
  const values = getMetricValues(metric);

  return (
    <article className="rounded-2xl border border-border bg-white px-5 py-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <h3 className="text-base font-semibold text-foreground">
          {formatUkrainianDateOnly(metric.recordedAt)}
        </h3>
        <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
          Метрики
        </span>
      </div>

      {values.length > 0 ? (
        <div className="mt-4 flex flex-wrap gap-2">
          {values.map((value) => (
            <span
              key={value}
              className="rounded-full border border-border bg-surface px-3 py-1 text-sm text-muted"
            >
              {value}
            </span>
          ))}
        </div>
      ) : null}

      {metric.note ? (
        <div className="mt-4 rounded-xl border border-border bg-surface px-4 py-3">
          <PlainText className="text-sm leading-6 text-muted">{metric.note}</PlainText>
        </div>
      ) : null}
    </article>
  );
}

function BodyMetricsSection({ metrics }: { metrics: BodyMetricEntry[] }) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold text-foreground">Метрики тіла</h2>

      {metrics.length === 0 ? (
        <div className="rounded-2xl border border-border bg-surface-strong/50 px-5 py-6">
          <p className="text-sm text-muted">Клієнт ще не додав метрики.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {metrics.map((metric) => (
            <BodyMetricCard key={metric.id} metric={metric} />
          ))}
        </div>
      )}
    </section>
  );
}

function ProgressPhotoCard({
  photo,
  onOpen,
}: {
  photo: ProgressPhoto;
  onOpen: (photo: ProgressPhoto) => void;
}) {
  return (
    <article className="overflow-hidden rounded-2xl border border-border bg-white">
      <button
        type="button"
        onClick={() => onOpen(photo)}
        className="block w-full bg-surface text-left"
      >
        <img
          src={photo.mediaAsset.deliveryUrl}
          alt={`Фото прогресу: ${getPhotoLabel(photo.label)}, ${formatUkrainianDateOnly(photo.takenAt)}`}
          className="aspect-[4/3] w-full object-cover"
          loading="lazy"
        />
      </button>
      <div className="space-y-2 px-4 py-4">
        <div className="flex flex-wrap items-center gap-2">
          <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
            {getPhotoLabel(photo.label)}
          </span>
          <span className="text-sm text-muted">
            {formatUkrainianDateOnly(photo.takenAt)}
          </span>
        </div>
        {photo.note ? (
          <PlainText className="text-sm leading-6 text-muted">{photo.note}</PlainText>
        ) : null}
      </div>
    </article>
  );
}

function ProgressPhotosSection({ photos }: { photos: ProgressPhoto[] }) {
  const [selectedPhoto, setSelectedPhoto] = useState<ProgressPhoto | null>(null);

  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold text-foreground">Фото прогресу</h2>

      {photos.length === 0 ? (
        <div className="rounded-2xl border border-border bg-surface-strong/50 px-5 py-6">
          <p className="text-sm text-muted">Клієнт ще не додав фото прогресу.</p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {photos.map((photo) => (
            <ProgressPhotoCard
              key={photo.id}
              photo={photo}
              onOpen={setSelectedPhoto}
            />
          ))}
        </div>
      )}

      {selectedPhoto ? (
        <MediaLightbox
          asset={selectedPhoto.mediaAsset}
          title={getPhotoLabel(selectedPhoto.label)}
          subtitle={formatUkrainianDateOnly(selectedPhoto.takenAt)}
          note={selectedPhoto.note ?? undefined}
          onClose={() => setSelectedPhoto(null)}
        />
      ) : null}
    </section>
  );
}

export function TrainerClientProgressTab({ progress }: TrainerClientProgressTabProps) {
  if (!progress) {
    return null;
  }

  return (
    <div className="space-y-6">
      <BodyMetricsSection metrics={progress.bodyMetrics} />
      <ProgressPhotosSection photos={progress.progressPhotos} />
    </div>
  );
}
