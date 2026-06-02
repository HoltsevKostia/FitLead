"use client";

import type { TrainerClientWorkoutLog } from "@/entities/user/model/types";
import { formatUkrainianDate } from "@/features/users/ui/trainer-client-date-formatting";
import { PlainText } from "@/shared/ui/plain-text";

interface TrainerClientWorkoutLogsTabProps {
  logs: TrainerClientWorkoutLog[] | null;
}

function getStatusLabel(status: string): string {
  if (status === "Completed") {
    return "Виконано";
  }

  if (status === "Skipped") {
    return "Пропущено";
  }

  return status;
}

function getStatusClassName(status: string): string {
  if (status === "Completed") {
    return "border-emerald-200 bg-emerald-50 text-emerald-800";
  }

  if (status === "Skipped") {
    return "border-amber-200 bg-amber-50 text-amber-800";
  }

  return "border-border bg-surface text-muted";
}

function getLogDate(log: TrainerClientWorkoutLog): string {
  return formatUkrainianDate(log.performedAtUtc ?? log.createdAtUtc);
}

function WorkoutLogCard({ log }: { log: TrainerClientWorkoutLog }) {
  return (
    <article className="rounded-2xl border border-border bg-white px-5 py-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`rounded-full border px-3 py-1 text-xs font-semibold ${getStatusClassName(
                log.status,
              )}`}
            >
              {getStatusLabel(log.status)}
            </span>
            {log.difficultyRating != null ? (
              <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
                Складність {log.difficultyRating}/10
              </span>
            ) : null}
          </div>

          <h2 className="break-words text-lg font-semibold text-foreground">
            {log.workoutName}
          </h2>
          <p className="break-words text-sm text-muted">{log.programTitle}</p>
          <p className="text-sm text-muted">
            Тиждень {log.weekNumber}, день {log.dayNumber}, #{log.orderInDay}
          </p>
        </div>

        <p className="text-sm font-medium text-foreground">{getLogDate(log)}</p>
      </div>

      {log.clientNote ? (
        <div className="mt-4 rounded-xl border border-border bg-surface px-4 py-3">
          <PlainText className="text-sm leading-6 text-muted">{log.clientNote}</PlainText>
        </div>
      ) : null}
    </article>
  );
}

export function TrainerClientWorkoutLogsTab({
  logs,
}: TrainerClientWorkoutLogsTabProps) {
  if (!logs) {
    return null;
  }

  if (logs.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-5 py-6">
        <p className="text-sm text-muted">Журнал тренувань поки порожній.</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {logs.map((log) => (
        <WorkoutLogCard key={log.logId} log={log} />
      ))}
    </div>
  );
}
