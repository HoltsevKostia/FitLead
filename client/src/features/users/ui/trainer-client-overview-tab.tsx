"use client";

import Link from "next/link";
import type { ReactNode } from "react";

import type {
  TrainerClientLastVideoReport,
  TrainerClientLastWorkoutLog,
  TrainerClientOverviewSummary,
} from "@/entities/user/model/types";
import {
  formatOptionalUkrainianDate,
  formatUkrainianDate,
  formatUkrainianDateOnly,
} from "@/features/users/ui/trainer-client-date-formatting";
import { MediaImage } from "@/shared/ui/media-image";
import { PlainText } from "@/shared/ui/plain-text";

interface TrainerClientOverviewTabProps {
  overview: TrainerClientOverviewSummary | null;
}

function getWorkoutStatusLabel(status: string): string {
  if (status === "Completed") {
    return "Виконано";
  }

  if (status === "Skipped") {
    return "Пропущено";
  }

  return status;
}

function getReportStatusLabel(status: string): string {
  if (status === "Submitted") {
    return "Очікує відгуку";
  }

  if (status === "Reviewed") {
    return "Переглянуто";
  }

  return status;
}

function getProgressPhotoLabel(label: string): string {
  const labels: Record<string, string> = {
    Front: "Спереду",
    Side: "Збоку",
    Back: "Ззаду",
    Other: "Інше",
  };

  return labels[label] ?? "Фото";
}

function OverviewCard({
  title,
  children,
  action,
}: {
  title: string;
  children: ReactNode;
  action?: ReactNode;
}) {
  return (
    <article className="rounded-2xl border border-border bg-white px-5 py-5">
      <div className="flex items-start justify-between gap-4">
        <h2 className="text-base font-semibold text-foreground">{title}</h2>
        {action}
      </div>
      <div className="mt-4">{children}</div>
    </article>
  );
}

function WorkoutCountsCard({ overview }: { overview: TrainerClientOverviewSummary }) {
  const counts = overview.workoutLogCounts;
  const items = [
    { label: "Виконано", value: counts.completed, className: "text-emerald-700" },
    { label: "Пропущено", value: counts.skipped, className: "text-amber-700" },
    { label: "Очікує", value: counts.pending, className: "text-sky-700" },
  ];

  return (
    <OverviewCard title="Стан тренувань">
      <div className="grid grid-cols-3 gap-2">
        {items.map((item) => (
          <div
            key={item.label}
            className="rounded-xl border border-border bg-surface px-3 py-3"
          >
            <p className={`text-2xl font-semibold ${item.className}`}>{item.value}</p>
            <p className="mt-1 text-xs text-muted">{item.label}</p>
          </div>
        ))}
      </div>
    </OverviewCard>
  );
}

function ActiveProgramCard({ overview }: { overview: TrainerClientOverviewSummary }) {
  const program = overview.activeProgram;

  return (
    <OverviewCard
      title="Активна програма"
      action={
        program ? (
          <Link
            href={`/training-programs/${program.programId}`}
            className="text-sm font-medium text-accent hover:text-accent-strong"
          >
            Відкрити
          </Link>
        ) : null
      }
    >
      {program ? (
        <div className="space-y-2">
          <p className="break-words text-lg font-semibold text-foreground">
            {program.programTitle}
          </p>
          <p className="text-sm text-muted">
            Призначено {formatUkrainianDate(program.assignedAtUtc)}
          </p>
          <p className="text-sm text-muted">
            Завершення: {formatOptionalUkrainianDate(program.expiresAtUtc)}
          </p>
          <p className="text-sm text-muted">
            Тренувань у програмі: {program.totalWorkouts}
          </p>
        </div>
      ) : (
        <p className="text-sm text-muted">Активну програму не призначено.</p>
      )}
    </OverviewCard>
  );
}

function LastWorkoutLogCard({ log }: { log: TrainerClientLastWorkoutLog | null }) {
  return (
    <OverviewCard title="Останній запис тренування">
      {log ? (
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {getWorkoutStatusLabel(log.status)}
            </span>
            {log.difficultyRating ? (
              <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
                {log.difficultyRating}/10
              </span>
            ) : null}
          </div>
          <p className="break-words text-lg font-semibold text-foreground">
            {log.workoutName}
          </p>
          <p className="text-sm text-muted">
            {log.programTitle} · тиждень {log.weekNumber}, день {log.dayNumber}
          </p>
          <p className="text-sm text-muted">
            {formatUkrainianDate(log.performedAtUtc ?? log.updatedAtUtc ?? log.createdAtUtc)}
          </p>
          {log.clientNote ? (
            <PlainText className="text-sm leading-6 text-muted">{log.clientNote}</PlainText>
          ) : null}
        </div>
      ) : (
        <p className="text-sm text-muted">Записів тренувань ще немає.</p>
      )}
    </OverviewCard>
  );
}

function LastVideoReportCard({
  report,
}: {
  report: TrainerClientLastVideoReport | null;
}) {
  return (
    <OverviewCard
      title="Останній відео-звіт"
      action={
        report ? (
          <Link
            href={`/chats/${report.chatId}/reports/${report.reportId}`}
            className="text-sm font-medium text-accent hover:text-accent-strong"
          >
            Відкрити
          </Link>
        ) : null
      }
    >
      {report ? (
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {getReportStatusLabel(report.status)}
            </span>
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {report.mediaCount} медіа
            </span>
          </div>
          <p className="break-words text-lg font-semibold text-foreground">
            {report.title}
          </p>
          <p className="text-sm text-muted">
            {formatUkrainianDate(report.createdAtUtc)}
          </p>
          {report.description ? (
            <PlainText className="line-clamp-3 text-sm leading-6 text-muted">
              {report.description}
            </PlainText>
          ) : null}
        </div>
      ) : (
        <p className="text-sm text-muted">Відео-звітів ще немає.</p>
      )}
    </OverviewCard>
  );
}

function ProgressCard({ overview }: { overview: TrainerClientOverviewSummary }) {
  const metric = overview.lastBodyMetric;
  const photo = overview.lastProgressPhoto;

  return (
    <OverviewCard
      title="Останній прогрес"
      action={
        <Link
          href="?tab=progress"
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Прогрес
        </Link>
      }
    >
      <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_160px]">
        <div className="space-y-2">
          {metric ? (
            <>
              <p className="text-sm font-medium text-foreground">
                Метрики від {formatUkrainianDateOnly(metric.recordedAt)}
              </p>
              <div className="flex flex-wrap gap-2 text-sm text-muted">
                {metric.weightKg ? <span>Вага: {metric.weightKg} кг</span> : null}
                {metric.waistCm ? <span>Талія: {metric.waistCm} см</span> : null}
                {metric.bodyFatPercent ? <span>Жир: {metric.bodyFatPercent}%</span> : null}
              </div>
              {metric.note ? (
                <PlainText className="text-sm leading-6 text-muted">{metric.note}</PlainText>
              ) : null}
            </>
          ) : (
            <p className="text-sm text-muted">Метрик ще немає.</p>
          )}
        </div>

        {photo ? (
          <div className="overflow-hidden rounded-xl border border-border bg-surface">
            <MediaImage
              src={photo.mediaAsset.deliveryUrl}
              alt={`Фото прогресу: ${getProgressPhotoLabel(photo.label)}, ${formatUkrainianDateOnly(photo.takenAt)}`}
              aspectRatio="4/5"
              className="h-36 w-full"
              sizes="160px"
            />
            <p className="px-3 py-2 text-xs text-muted">
              {formatUkrainianDateOnly(photo.takenAt)}
            </p>
          </div>
        ) : null}
      </div>
    </OverviewCard>
  );
}

export function TrainerClientOverviewTab({
  overview,
}: TrainerClientOverviewTabProps) {
  if (!overview) {
    return null;
  }

  return (
    <div className="grid gap-4 xl:grid-cols-2">
      <ActiveProgramCard overview={overview} />
      <WorkoutCountsCard overview={overview} />
      <LastWorkoutLogCard log={overview.lastWorkoutLog} />
      <LastVideoReportCard report={overview.lastVideoReport} />
      <div className="xl:col-span-2">
        <ProgressCard overview={overview} />
      </div>
    </div>
  );
}
