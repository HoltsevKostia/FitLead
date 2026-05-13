"use client";

import Link from "next/link";
import { useMemo, useState } from "react";

import type {
  ClientAssignedTrainingProgramDetails,
  ClientAssignedTrainingProgramWorkout,
} from "@/entities/training-program/model/types";
import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { ExerciseMediaPreview } from "@/features/exercises/ui/exercise-media-preview";

interface ClientAssignedTrainingProgramDetailViewProps {
  program: ClientAssignedTrainingProgramDetails;
}

function buildRange(count: number): number[] {
  return Array.from({ length: count }, (_, index) => index + 1);
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

function formatLoad(loadKg: number | null): string {
  if (loadKg === null) {
    return "Без ваги";
  }

  return `${loadKg} кг`;
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

function ExerciseCard({ exercise }: { exercise: WorkoutExerciseDetails }) {
  return (
    <article className="rounded-xl border border-border bg-surface px-4 py-4">
      <div className="space-y-3">
        <div className="flex flex-wrap items-center gap-2">
          <span className="inline-flex rounded-full border border-border bg-white px-3 py-1 text-xs font-semibold text-muted">
            {exercise.order}
          </span>
          {exercise.exerciseMuscleGroup ? (
            <span className="inline-flex rounded-full border border-border bg-white px-3 py-1 text-xs font-medium text-muted">
              {muscleGroupLabels[exercise.exerciseMuscleGroup]}
            </span>
          ) : null}
          {exercise.exerciseEquipment ? (
            <span className="inline-flex rounded-full border border-border bg-white px-3 py-1 text-xs font-medium text-muted">
              {equipmentLabels[exercise.exerciseEquipment]}
            </span>
          ) : null}
        </div>

        <div className="space-y-2">
          <h4 className="text-base font-semibold text-foreground">{exercise.exerciseName}</h4>
          <p className="text-sm leading-6 text-muted">
            {exercise.exerciseDescription || "Опис поки не додано."}
          </p>
          <ExerciseMediaPreview mediaUrl={exercise.exerciseMediaUrl} />
        </div>

        <div className="grid grid-cols-2 gap-2 text-sm sm:grid-cols-4">
          <div className="rounded-xl border border-border bg-white px-3 py-3">
            <p className="text-xs text-muted">Підходи</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.sets}</p>
          </div>
          <div className="rounded-xl border border-border bg-white px-3 py-3">
            <p className="text-xs text-muted">Повторення</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.repetitions}</p>
          </div>
          <div className="rounded-xl border border-border bg-white px-3 py-3">
            <p className="text-xs text-muted">Вага</p>
            <p className="mt-1 font-semibold text-foreground">{formatLoad(exercise.loadKg)}</p>
          </div>
          <div className="rounded-xl border border-border bg-white px-3 py-3">
            <p className="text-xs text-muted">Відпочинок</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.restSeconds} сек</p>
          </div>
        </div>

        {exercise.trainerNote ? (
          <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3">
            <p className="text-xs font-semibold uppercase text-amber-900">Нотатка тренера</p>
            <p className="mt-1 whitespace-pre-line text-sm leading-6 text-amber-950">
              {exercise.trainerNote}
            </p>
          </div>
        ) : null}
      </div>
    </article>
  );
}

function WorkoutCard({ workout }: { workout: ClientAssignedTrainingProgramWorkout }) {
  const exercises = [...workout.exercises].sort((first, second) => first.order - second.order);

  return (
    <article className="rounded-xl border border-border bg-white px-4 py-4">
      <div className="space-y-4">
        <div className="flex flex-wrap items-center gap-2">
          <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
            {workout.orderInDay}
          </span>
          <span className="text-xs font-medium text-muted">Тренування</span>
        </div>

        <div>
          <h3 className="text-lg font-semibold text-foreground">{workout.workoutName}</h3>
          <p className="mt-1 text-sm text-muted">
            {exercises.length > 0
              ? `${exercises.length} вправ`
              : "Вправи поки не додано"}
          </p>
        </div>

        {exercises.length > 0 ? (
          <div className="space-y-3">
            {exercises.map((exercise) => (
              <ExerciseCard key={exercise.workoutExerciseId} exercise={exercise} />
            ))}
          </div>
        ) : null}
      </div>
    </article>
  );
}

function DayCard({
  dayNumber,
  entries,
}: {
  dayNumber: number;
  entries: ClientAssignedTrainingProgramWorkout[];
}) {
  return (
    <article className="rounded-2xl border border-border bg-surface px-5 py-5">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-foreground">День {dayNumber}</h2>
        <span className="rounded-full border border-border bg-white px-3 py-1 text-xs font-medium text-muted">
          {entries.length > 0 ? `${entries.length} трен.` : "Порожньо"}
        </span>
      </div>

      {entries.length === 0 ? (
        <div className="rounded-xl border border-dashed border-border bg-white/70 px-4 py-5 text-sm text-muted">
          Тренування не додано
        </div>
      ) : (
        <div className="space-y-3">
          {entries.map((entry) => (
            <WorkoutCard key={entry.id} workout={entry} />
          ))}
        </div>
      )}
    </article>
  );
}

export function ClientAssignedTrainingProgramDetailView({
  program,
}: ClientAssignedTrainingProgramDetailViewProps) {
  const [selectedWeek, setSelectedWeek] = useState(1);
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

      <div className="space-y-3">
        <label className="block text-sm font-medium text-foreground md:hidden" htmlFor="week-select">
          Тиждень
        </label>
        <select
          id="week-select"
          value={selectedWeek}
          onChange={(event) => setSelectedWeek(Number(event.target.value))}
          className="w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none transition focus:border-accent md:hidden"
        >
          {weeks.map((weekNumber) => (
            <option key={weekNumber} value={weekNumber}>
              Тиждень {weekNumber}
            </option>
          ))}
        </select>

        <div className="hidden flex-wrap gap-2 md:flex" role="tablist" aria-label="Тижні програми">
          {weeks.map((weekNumber) => {
            const isSelected = selectedWeek === weekNumber;

            return (
              <button
                key={weekNumber}
                type="button"
                role="tab"
                aria-selected={isSelected}
                onClick={() => setSelectedWeek(weekNumber)}
                className={`rounded-full border px-4 py-2 text-sm font-medium transition ${
                  isSelected
                    ? "border-accent bg-accent text-white"
                    : "border-border bg-white text-foreground hover:bg-surface-strong"
                }`}
              >
                Тиждень {weekNumber}
              </button>
            );
          })}
        </div>
      </div>

      <div className="grid gap-4 xl:grid-cols-2">
        {days.map((dayNumber) => (
          <DayCard
            key={dayNumber}
            dayNumber={dayNumber}
            entries={getEntriesForDay(program.workouts, selectedWeek, dayNumber)}
          />
        ))}
      </div>
    </section>
  );
}
