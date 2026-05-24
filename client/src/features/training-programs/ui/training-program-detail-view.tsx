"use client";

import Link from "next/link";
import { useMemo, useState } from "react";

import type {
  TrainingProgram,
  TrainingProgramAssignment,
  TrainingProgramWorkout,
} from "@/entities/training-program/model/types";
import type { TrainerClient } from "@/entities/user/model/types";
import type { Workout } from "@/entities/workout/model/types";
import { AddWorkoutToProgramDayForm } from "@/features/training-programs/ui/add-workout-to-program-day-form";
import { AssignTrainingProgramToClientForm } from "@/features/training-programs/ui/assign-training-program-to-client-form";
import { TrainingProgramAssignmentList } from "@/features/training-programs/ui/training-program-assignment-list";
import { TrainingProgramWorkoutEntryActions } from "@/features/training-programs/ui/training-program-workout-entry-actions";

interface TrainingProgramDetailViewProps {
  program: TrainingProgram;
  workouts: TrainingProgramWorkout[];
  availableWorkouts: Workout[];
  clients: TrainerClient[];
  assignments: TrainingProgramAssignment[];
}

function buildRange(count: number): number[] {
  return Array.from({ length: count }, (_, index) => index + 1);
}

function getEntriesForDay(
  workouts: TrainingProgramWorkout[],
  weekNumber: number,
  dayNumber: number,
): TrainingProgramWorkout[] {
  return workouts
    .filter((entry) => entry.weekNumber === weekNumber && entry.dayNumber === dayNumber)
    .sort((first, second) => first.orderInDay - second.orderInDay);
}

function WorkoutCard({
  programId,
  entry,
}: {
  programId: string;
  entry: TrainingProgramWorkout;
}) {
  return (
    <article className="rounded-xl border border-border bg-white px-4 py-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {entry.orderInDay}
            </span>
            <span className="text-xs font-medium text-muted">Тренування</span>
          </div>
          <h3 className="mt-2 text-base font-semibold text-foreground">{entry.workoutName}</h3>
        </div>

        <div className="flex flex-col gap-2 sm:items-end">
          <Link
            href={`/workouts/${entry.workoutId}`}
            className="w-fit rounded-full border border-border px-3 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
          >
            Відкрити
          </Link>
          <TrainingProgramWorkoutEntryActions programId={programId} entryId={entry.id} />
        </div>
      </div>
    </article>
  );
}

function DayCard({
  programId,
  weekNumber,
  dayNumber,
  entries,
  availableWorkouts,
}: {
  programId: string;
  weekNumber: number;
  dayNumber: number;
  entries: TrainingProgramWorkout[];
  availableWorkouts: Workout[];
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
            <WorkoutCard key={entry.id} programId={programId} entry={entry} />
          ))}
        </div>
      )}

      <div className="mt-4">
        <AddWorkoutToProgramDayForm
          programId={programId}
          weekNumber={weekNumber}
          dayNumber={dayNumber}
          availableWorkouts={availableWorkouts}
        />
      </div>
    </article>
  );
}

export function TrainingProgramDetailView({
  program,
  workouts,
  availableWorkouts,
  clients,
  assignments,
}: TrainingProgramDetailViewProps) {
  const [selectedWeek, setSelectedWeek] = useState(1);
  const weeks = useMemo(() => buildRange(program.weeksCount), [program.weeksCount]);
  const days = useMemo(() => buildRange(program.daysPerWeek), [program.daysPerWeek]);

  return (
    <section className="space-y-6">
      <Link
        href="/training-programs"
        className="text-sm font-medium text-accent hover:text-accent-strong"
      >
        Назад до програм
      </Link>

      <div className="space-y-2">
        <h1 className="text-3xl font-semibold tracking-tight">{program.title}</h1>
        <p className="max-w-3xl text-muted">
          {program.weeksCount} тиж. · {program.daysPerWeek} дн./тиждень
        </p>
      </div>

      <AssignTrainingProgramToClientForm programId={program.id} clients={clients} />
      <TrainingProgramAssignmentList programId={program.id} assignments={assignments} />

      <div className="space-y-3">
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

      <div className="grid gap-4 xl:grid-cols-2">
        {days.map((dayNumber) => (
          <DayCard
            key={dayNumber}
            programId={program.id}
            weekNumber={selectedWeek}
            dayNumber={dayNumber}
            entries={getEntriesForDay(workouts, selectedWeek, dayNumber)}
            availableWorkouts={availableWorkouts}
          />
        ))}
      </div>
    </section>
  );
}
