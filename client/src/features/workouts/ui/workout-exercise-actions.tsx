"use client";

import { type SubmitEvent, useState } from "react";
import { useRouter } from "next/navigation";

import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";
import { mapWorkoutMutationError } from "@/features/workouts/model/error-mapping";
import { parseWorkoutExercisePrescription } from "@/features/workouts/model/workout-exercise-prescription";
import { WorkoutExercisePrescriptionFields } from "@/features/workouts/ui/workout-exercise-prescription-fields";
import { workoutsApi } from "@/lib/api/clients/workouts-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface WorkoutExerciseActionsProps {
  workoutId: string;
  exercise: WorkoutExerciseDetails;
}

function formatOptionalNumber(value: number | null): string {
  return value === null ? "" : String(value);
}

export function WorkoutExerciseActions({
  workoutId,
  exercise,
}: WorkoutExerciseActionsProps) {
  const router = useRouter();
  const [isEditing, setIsEditing] = useState(false);
  const [sets, setSets] = useState(String(exercise.sets));
  const [repetitions, setRepetitions] = useState(String(exercise.repetitions));
  const [loadKg, setLoadKg] = useState(formatOptionalNumber(exercise.loadKg));
  const [restSeconds, setRestSeconds] = useState(String(exercise.restSeconds));
  const [trainerNote, setTrainerNote] = useState(exercise.trainerNote ?? "");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  function resetFields() {
    setSets(String(exercise.sets));
    setRepetitions(String(exercise.repetitions));
    setLoadKg(formatOptionalNumber(exercise.loadKg));
    setRestSeconds(String(exercise.restSeconds));
    setTrainerNote(exercise.trainerNote ?? "");
  }

  function toggleEdit() {
    setIsEditing((current) => {
      const next = !current;

      if (!next) {
        resetFields();
      }

      return next;
    });
    setSubmitError(null);
  }

  async function handleEditSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const prescription = parseWorkoutExercisePrescription({
      sets,
      repetitions,
      loadKg,
      restSeconds,
      trainerNote,
    });

    if (!prescription.payload) {
      setSubmitError(prescription.error);
      return;
    }

    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await workoutsApi.updateExercise(
        workoutId,
        exercise.workoutExerciseId,
        prescription.payload,
      );
      setIsEditing(false);
      router.refresh();
    } catch (error) {
      setSubmitError(mapWorkoutMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleRemove() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await workoutsApi.removeExercise(workoutId, exercise.workoutExerciseId);
      router.refresh();
    } catch (error) {
      setSubmitError(mapWorkoutMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="mt-4 space-y-3">
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          onClick={toggleEdit}
          disabled={isSubmitting}
          className="rounded-full border border-border px-4 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isEditing ? "Скасувати" : "Редагувати параметри"}
        </button>
        <button
          type="button"
          onClick={handleRemove}
          disabled={isSubmitting}
          className="rounded-full border border-red-200 px-4 py-2 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Обробка..." : "Видалити з тренування"}
        </button>
      </div>

      <FormAlert message={submitError} />

      {isEditing ? (
        <form
          onSubmit={handleEditSubmit}
          className="space-y-4 rounded-2xl border border-border bg-surface px-4 py-4"
        >
          <WorkoutExercisePrescriptionFields
            idPrefix={`edit-workout-exercise-${exercise.workoutExerciseId}`}
            sets={sets}
            repetitions={repetitions}
            loadKg={loadKg}
            restSeconds={restSeconds}
            trainerNote={trainerNote}
            isSubmitting={isSubmitting}
            onSetsChange={setSets}
            onRepetitionsChange={setRepetitions}
            onLoadKgChange={setLoadKg}
            onRestSecondsChange={setRestSeconds}
            onTrainerNoteChange={setTrainerNote}
          />

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isSubmitting ? "Зберігаємо..." : "Зберегти"}
          </button>
        </form>
      ) : null}
    </div>
  );
}
