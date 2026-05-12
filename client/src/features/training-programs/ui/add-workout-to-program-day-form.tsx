"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { type SubmitEvent, useMemo, useState } from "react";

import type { Workout } from "@/entities/workout/model/types";
import { mapTrainingProgramMutationError } from "@/features/training-programs/model/error-mapping";
import { trainingProgramsApi } from "@/lib/api/clients/training-programs-api";
import { fieldInputClassName } from "@/shared/forms/field-styles";
import { FormAlert } from "@/shared/forms/form-alert";

interface AddWorkoutToProgramDayFormProps {
  programId: string;
  weekNumber: number;
  dayNumber: number;
  availableWorkouts: Workout[];
}

function getFilteredWorkouts(workouts: Workout[], search: string): Workout[] {
  const normalizedSearch = search.trim().toLowerCase();

  if (!normalizedSearch) {
    return workouts;
  }

  return workouts.filter((workout) => workout.name.toLowerCase().includes(normalizedSearch));
}

export function AddWorkoutToProgramDayForm({
  programId,
  weekNumber,
  dayNumber,
  availableWorkouts,
}: AddWorkoutToProgramDayFormProps) {
  const router = useRouter();
  const [isOpen, setIsOpen] = useState(false);
  const [selectedWorkoutId, setSelectedWorkoutId] = useState("");
  const [search, setSearch] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const visibleWorkouts = useMemo(
    () => getFilteredWorkouts(availableWorkouts, search),
    [availableWorkouts, search],
  );
  const selectedWorkout =
    availableWorkouts.find((workout) => workout.id === selectedWorkoutId) ?? null;

  function closePicker() {
    setIsOpen(false);
    setSelectedWorkoutId("");
    setSearch("");
    setFormError(null);
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedWorkout) {
      setFormError("Виберіть тренування.");
      return;
    }

    setIsSubmitting(true);
    setFormError(null);

    try {
      await trainingProgramsApi.addWorkout(programId, {
        workoutId: selectedWorkout.id,
        weekNumber,
        dayNumber,
      });
      closePicker();
      router.refresh();
    } catch (error) {
      setFormError(mapTrainingProgramMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="space-y-3">
      <button
        type="button"
        onClick={() => (isOpen ? closePicker() : setIsOpen(true))}
        disabled={isSubmitting}
        className="rounded-full bg-accent px-4 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isOpen ? "Закрити" : "Додати тренування"}
      </button>

      {isOpen ? (
        <form
          onSubmit={handleSubmit}
          className="space-y-4 rounded-2xl border border-border bg-white px-4 py-4"
        >
          <p className="text-sm text-muted">
            Додавання до тижня {weekNumber}, дня {dayNumber}
          </p>

          {availableWorkouts.length === 0 ? (
            <div className="rounded-xl border border-dashed border-border bg-surface px-4 py-4 text-sm text-muted">
              <p>У бібліотеці ще немає тренувань.</p>
              <Link
                href="/workouts"
                className="mt-2 inline-flex font-medium text-accent hover:text-accent-strong"
              >
                Перейти до тренувань
              </Link>
            </div>
          ) : (
            <>
              <label
                className="sr-only"
                htmlFor={`workout-picker-search-${weekNumber}-${dayNumber}`}
              >
                Пошук тренування
              </label>
              <input
                id={`workout-picker-search-${weekNumber}-${dayNumber}`}
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                disabled={isSubmitting}
                placeholder="Пошук тренування"
                className={fieldInputClassName}
              />

              <div className="max-h-72 space-y-2 overflow-y-auto rounded-2xl border border-border bg-surface p-2">
                {visibleWorkouts.length === 0 ? (
                  <p className="px-3 py-4 text-sm text-muted">Тренувань не знайдено.</p>
                ) : null}

                {visibleWorkouts.map((workout) => {
                  const isSelected = workout.id === selectedWorkoutId;

                  return (
                    <button
                      key={workout.id}
                      type="button"
                      aria-pressed={isSelected}
                      onClick={() => setSelectedWorkoutId(workout.id)}
                      disabled={isSubmitting}
                      className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                        isSelected
                          ? "border-accent bg-emerald-50"
                          : "border-border bg-white hover:border-accent"
                      }`}
                    >
                      <span className="block truncate text-sm font-semibold text-foreground">
                        {workout.name}
                      </span>
                    </button>
                  );
                })}
              </div>

              {selectedWorkout ? (
                <p className="text-sm text-muted">
                  Обрано:{" "}
                  <span className="font-medium text-foreground">{selectedWorkout.name}</span>
                </p>
              ) : null}
            </>
          )}

          <FormAlert message={formError} />

          <div className="flex flex-col gap-3 sm:flex-row">
            <button
              type="submit"
              disabled={isSubmitting || availableWorkouts.length === 0 || !selectedWorkout}
              className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              {isSubmitting ? "Додаємо..." : "Додати"}
            </button>
            <button
              type="button"
              onClick={closePicker}
              disabled={isSubmitting}
              className="rounded-full border border-border px-5 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              Скасувати
            </button>
          </div>
        </form>
      ) : null}
    </div>
  );
}
