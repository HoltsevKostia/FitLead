"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import type { WorkoutLogPreview, WorkoutLogStatus } from "@/entities/training-program/model/types";
import { mapWorkoutLogMutationError } from "@/features/workout-logs/model/error-mapping";
import { workoutLogsApi } from "@/lib/api/clients/workout-logs-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldErrorClassName, fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface WorkoutLogSectionProps {
  assignmentId: string;
  programWorkoutId: string;
  log: WorkoutLogPreview | null;
}

function formatLogDate(value: string): string {
  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatDifficultyLabel(rating: number): string {
  return `${rating}/10`;
}

function getStatusBadgeClassName(status: string): string {
  if (status === "Completed") {
    return "border-emerald-200 bg-emerald-50 text-emerald-700";
  }

  if (status === "Skipped") {
    return "border-amber-200 bg-amber-50 text-amber-800";
  }

  return "border-border bg-surface text-muted";
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

function parseDifficultyRating(value: string): number | undefined {
  if (value.trim() === "") {
    return undefined;
  }

  const parsed = Number(value);

  if (!Number.isInteger(parsed) || parsed < 1 || parsed > 10) {
    return undefined;
  }

  return parsed;
}

function WorkoutLogSummary({ log }: { log: WorkoutLogPreview }) {
  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold text-foreground">Результат тренування</h2>
      <div className="rounded-lg border border-border bg-white px-4 py-4 sm:px-5">
        <div className="space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold ${getStatusBadgeClassName(log.status)}`}
            >
              {getStatusLabel(log.status)}
            </span>
            {log.performedAtUtc ? (
              <span className="text-sm text-muted">{formatLogDate(log.performedAtUtc)}</span>
            ) : null}
            {log.difficultyRating !== null ? (
              <span className="text-sm text-muted">
                Складність {formatDifficultyLabel(log.difficultyRating)}
              </span>
            ) : null}
          </div>
          {log.clientNote ? (
            <p className="whitespace-pre-wrap break-words text-sm leading-6 text-foreground">
              {log.clientNote}
            </p>
          ) : null}
        </div>
      </div>
    </div>
  );
}

function ActionButtons({
  onSelect,
  disabled,
}: {
  onSelect: (status: WorkoutLogStatus) => void;
  disabled: boolean;
}) {
  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold text-foreground">Результат тренування</h2>
      <div className="flex flex-wrap gap-3">
        <button
          type="button"
          onClick={() => onSelect("Completed")}
          disabled={disabled}
          className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-5 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          Виконано
        </button>
        <button
          type="button"
          onClick={() => onSelect("Skipped")}
          disabled={disabled}
          className="inline-flex min-h-10 items-center justify-center rounded-lg border border-amber-200 bg-white px-5 py-2 text-sm font-semibold text-amber-800 transition hover:bg-amber-50 disabled:cursor-not-allowed disabled:opacity-70"
        >
          Пропустити
        </button>
      </div>
    </div>
  );
}

function CompletedForm({
  difficultyRating,
  clientNote,
  onDifficultyChange,
  onClientNoteChange,
  onBack,
  onSubmit,
  isSubmitting,
  submitError,
}: {
  difficultyRating: string;
  clientNote: string;
  onDifficultyChange: (value: string) => void;
  onClientNoteChange: (value: string) => void;
  onBack: () => void;
  onSubmit: () => void;
  isSubmitting: boolean;
  submitError: string | null;
}) {
  const isDifficultyValid =
    difficultyRating.trim() === "" ||
    (Number.isInteger(Number(difficultyRating)) &&
      Number(difficultyRating) >= 1 &&
      Number(difficultyRating) <= 10);
  const canSubmit = !isSubmitting && isDifficultyValid;

  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold text-foreground">Результат тренування</h2>
      <div className="rounded-lg border border-border bg-white px-4 py-4 sm:px-5">
        <div className="space-y-4">
          <span className="inline-flex rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
            Виконано
          </span>

          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor="difficulty-rating">
              Складність <span className="text-muted">(1–10, необов’язково)</span>
            </label>
            <input
              id="difficulty-rating"
              type="number"
              min={1}
              max={10}
              step={1}
              value={difficultyRating}
              onChange={(event) => onDifficultyChange(event.currentTarget.value)}
              disabled={isSubmitting}
              placeholder="від 1 до 10"
              aria-invalid={!isDifficultyValid}
              className={fieldInputClassName}
            />
            {!isDifficultyValid ? (
              <p className={fieldErrorClassName}>Оцінка складності має бути від 1 до 10.</p>
            ) : null}
          </div>

          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor="client-note-completed">
              Коментар <span className="text-muted">(необов’язково)</span>
            </label>
            <textarea
              id="client-note-completed"
              rows={3}
              maxLength={1000}
              value={clientNote}
              onChange={(event) => onClientNoteChange(event.currentTarget.value)}
              disabled={isSubmitting}
              placeholder="Нотатки про тренування..."
              className={`${fieldInputClassName} resize-y`}
            />
          </div>

          <FormAlert message={submitError} />

          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={onBack}
              disabled={isSubmitting}
              className="inline-flex min-h-10 items-center justify-center rounded-lg border border-border bg-white px-5 py-2 text-sm font-medium text-foreground transition hover:bg-surface disabled:cursor-not-allowed disabled:opacity-70"
            >
              Назад
            </button>
            <button
              type="button"
              onClick={onSubmit}
              disabled={!canSubmit}
              className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-5 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              {isSubmitting ? "Зберігаємо..." : "Зберегти"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function SkippedForm({
  clientNote,
  onClientNoteChange,
  onBack,
  onSubmit,
  isSubmitting,
  submitError,
}: {
  clientNote: string;
  onClientNoteChange: (value: string) => void;
  onBack: () => void;
  onSubmit: () => void;
  isSubmitting: boolean;
  submitError: string | null;
}) {
  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold text-foreground">Результат тренування</h2>
      <div className="rounded-lg border border-border bg-white px-4 py-4 sm:px-5">
        <div className="space-y-4">
          <span className="inline-flex rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-800">
            Пропущено
          </span>

          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor="client-note-skipped">
              Коментар <span className="text-muted">(необов’язково)</span>
            </label>
            <textarea
              id="client-note-skipped"
              rows={3}
              maxLength={1000}
              value={clientNote}
              onChange={(event) => onClientNoteChange(event.currentTarget.value)}
              disabled={isSubmitting}
              placeholder="Чому пропустили?"
              className={`${fieldInputClassName} resize-y`}
            />
          </div>

          <FormAlert message={submitError} />

          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={onBack}
              disabled={isSubmitting}
              className="inline-flex min-h-10 items-center justify-center rounded-lg border border-border bg-white px-5 py-2 text-sm font-medium text-foreground transition hover:bg-surface disabled:cursor-not-allowed disabled:opacity-70"
            >
              Назад
            </button>
            <button
              type="button"
              onClick={onSubmit}
              disabled={isSubmitting}
              className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-5 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              {isSubmitting ? "Зберігаємо..." : "Зберегти"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export function WorkoutLogSection({ assignmentId, programWorkoutId, log }: WorkoutLogSectionProps) {
  const router = useRouter();
  const [formStatus, setFormStatus] = useState<WorkoutLogStatus | null>(null);
  const [difficultyRating, setDifficultyRating] = useState("");
  const [clientNote, setClientNote] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  if (log) {
    return <WorkoutLogSummary log={log} />;
  }

  if (formStatus === "Completed") {
    return (
      <CompletedForm
        difficultyRating={difficultyRating}
        clientNote={clientNote}
        onDifficultyChange={setDifficultyRating}
        onClientNoteChange={setClientNote}
        onBack={() => {
          setFormStatus(null);
          setSubmitError(null);
        }}
        onSubmit={() => handleSubmit("Completed")}
        isSubmitting={isSubmitting}
        submitError={submitError}
      />
    );
  }

  if (formStatus === "Skipped") {
    return (
      <SkippedForm
        clientNote={clientNote}
        onClientNoteChange={setClientNote}
        onBack={() => {
          setFormStatus(null);
          setSubmitError(null);
        }}
        onSubmit={() => handleSubmit("Skipped")}
        isSubmitting={isSubmitting}
        submitError={submitError}
      />
    );
  }

  return (
    <ActionButtons
      onSelect={(status) => {
        setFormStatus(status);
        setSubmitError(null);
      }}
      disabled={isSubmitting}
    />
  );

  async function handleSubmit(status: WorkoutLogStatus) {
    if (isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setSubmitError(null);

    try {
      const normalizedNote = clientNote.trim() || undefined;

      if (status === "Completed") {
        const parsedDifficulty = parseDifficultyRating(difficultyRating);

        if (difficultyRating.trim() !== "" && parsedDifficulty === undefined) {
          setSubmitError("Оцінка складності має бути цілим числом від 1 до 10.");
          setIsSubmitting(false);
          return;
        }

        await workoutLogsApi.logWorkout(assignmentId, programWorkoutId, {
          status: "Completed",
          difficultyRating: parsedDifficulty,
          clientNote: normalizedNote,
        });
      } else {
        await workoutLogsApi.logWorkout(assignmentId, programWorkoutId, {
          status: "Skipped",
          clientNote: normalizedNote,
        });
      }

      router.refresh();
    } catch (error) {
      setSubmitError(mapWorkoutLogMutationError(error));
      setIsSubmitting(false);
    }
  }
}
