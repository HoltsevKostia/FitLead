"use client";

import Link from "next/link";
import { useMemo, useState } from "react";

import type {
  ClientAssignedTrainingProgramDetails,
  ClientAssignedTrainingProgramWorkout,
} from "@/entities/training-program/model/types";

interface ClientAssignedTrainingProgramDetailViewProps {
  program: ClientAssignedTrainingProgramDetails;
  initialWeek?: number;
}

function buildRange(count: number): number[] {
  return Array.from({ length: count }, (_, index) => index + 1);
}

function getInitialWeek(initialWeek: number | undefined, weeksCount: number): number {
  if (!initialWeek || initialWeek < 1 || initialWeek > weeksCount) {
    return 1;
  }

  return initialWeek;
}

function formatDate(value: string | null): string {
  if (!value) {
    return "Безстроково";
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

function getEntriesForDay(
  workouts: ClientAssignedTrainingProgramWorkout[],
  weekNumber: number,
  dayNumber: number,
): ClientAssignedTrainingProgramWorkout[] {
  return workouts
    .filter((entry) => entry.weekNumber === weekNumber && entry.dayNumber === dayNumber)
    .sort((first, second) => first.orderInDay - second.orderInDay);
}

function buildProgramWeekHref(assignmentId: string, weekNumber: number): string {
  return `/client/training-programs/${assignmentId}?week=${weekNumber}`;
}

function buildWorkoutHref(
  assignmentId: string,
  programWorkoutId: string,
  weekNumber: number,
): string {
  const returnTo = buildProgramWeekHref(assignmentId, weekNumber);
  return `/client/training-programs/${assignmentId}/workouts/${programWorkoutId}?returnTo=${encodeURIComponent(returnTo)}`;
}

function WorkoutCard({
  assignmentId,
  selectedWeek,
  workout,
}: {
  assignmentId: string;
  selectedWeek: number;
  workout: ClientAssignedTrainingProgramWorkout;
}) {
  const exerciseCount = workout.exercises.length;

  return (
    <article className="rounded-lg border border-border bg-white px-4 py-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2 text-xs text-muted">
            <span className="inline-flex rounded-full border border-border bg-surface px-2.5 py-1 font-semibold">
              #{workout.orderInDay}
            </span>
            <span>Тиждень {workout.weekNumber}</span>
            <span>День {workout.dayNumber}</span>
          </div>

          <div>
            <h3 className="break-words text-base font-semibold text-foreground">
              {workout.workoutName}
            </h3>
            <p className="mt-1 text-sm text-muted">
              {exerciseCount > 0 ? `${exerciseCount} вправ` : "Вправи поки не додано"}
            </p>
          </div>
        </div>

        <Link
          href={buildWorkoutHref(assignmentId, workout.id, selectedWeek)}
          className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-4 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong sm:shrink-0"
        >
          Відкрити
        </Link>
      </div>
    </article>
  );
}

function DayCard({
  assignmentId,
  selectedWeek,
  dayNumber,
  entries,
}: {
  assignmentId: string;
  selectedWeek: number;
  dayNumber: number;
  entries: ClientAssignedTrainingProgramWorkout[];
}) {
  return (
    <article className="rounded-lg border border-border bg-surface px-4 py-4 sm:px-5">
      <div className="mb-3 flex items-center justify-between gap-3">
        <h2 className="text-base font-semibold text-foreground">День {dayNumber}</h2>
        <span className="rounded-full border border-border bg-white px-3 py-1 text-xs font-medium text-muted">
          {entries.length > 0 ? `${entries.length} трен.` : "Порожньо"}
        </span>
      </div>

      {entries.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border bg-white/70 px-4 py-3 text-sm text-muted">
          Тренування не додано
        </div>
      ) : (
        <div className="space-y-2">
          {entries.map((entry) => (
            <WorkoutCard
              key={entry.id}
              assignmentId={assignmentId}
              selectedWeek={selectedWeek}
              workout={entry}
            />
          ))}
        </div>
      )}
    </article>
  );
}

export function ClientAssignedTrainingProgramDetailView({
  program,
  initialWeek,
}: ClientAssignedTrainingProgramDetailViewProps) {
  const [selectedWeek, setSelectedWeek] = useState(() =>
    getInitialWeek(initialWeek, program.weeksCount),
  );
  const weeks = useMemo(() => buildRange(program.weeksCount), [program.weeksCount]);
  const days = useMemo(() => buildRange(program.daysPerWeek), [program.daysPerWeek]);

  return (
    <section className="space-y-6">
      <Link
        href="/client/training-programs"
        className="text-sm font-medium text-accent hover:text-accent-strong"
      >
        Назад до моїх програм
      </Link>

      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">{program.title}</h1>
        <p className="max-w-3xl text-muted">
          Тренер: {program.trainerName} · {program.weeksCount} тиж. ·{" "}
          {program.daysPerWeek} дн./тиждень · Доступ: {formatDate(program.expiresAtUtc)}
        </p>
      </div>

      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground md:hidden" htmlFor="week-select">
          Тиждень
        </label>
        <select
          id="week-select"
          value={selectedWeek}
          onChange={(event) => setSelectedWeek(Number(event.target.value))}
          className="w-full rounded-lg border border-border bg-white px-4 py-3 outline-none transition focus:border-accent md:hidden"
        >
          {weeks.map((weekNumber) => (
            <option key={weekNumber} value={weekNumber}>
              Тиждень {weekNumber}
            </option>
          ))}
        </select>

        <div className="hidden items-center gap-2 md:flex">
          <span className="text-sm font-medium text-foreground">Тиждень:</span>
          <div className="flex flex-wrap gap-2" role="tablist" aria-label="Тижні програми">
            {weeks.map((weekNumber) => {
              const isSelected = selectedWeek === weekNumber;

              return (
                <button
                  key={weekNumber}
                  type="button"
                  role="tab"
                  aria-selected={isSelected}
                  onClick={() => setSelectedWeek(weekNumber)}
                  className={`h-9 min-w-9 rounded-lg border px-3 text-sm font-medium transition ${
                    isSelected
                      ? "border-accent bg-accent text-white"
                      : "border-border bg-white text-foreground hover:bg-surface-strong"
                  }`}
                >
                  {weekNumber}
                </button>
              );
            })}
          </div>
        </div>
      </div>

      <div className="mx-auto grid max-w-4xl gap-3">
        {days.map((dayNumber) => (
          <DayCard
            key={dayNumber}
            assignmentId={program.assignmentId}
            selectedWeek={selectedWeek}
            dayNumber={dayNumber}
            entries={getEntriesForDay(program.workouts, selectedWeek, dayNumber)}
          />
        ))}
      </div>
    </section>
  );
}
