"use client";

import { type SubmitEvent, useState } from "react";
import { useRouter } from "next/navigation";

import type { Exercise } from "@/entities/exercise/model/types";
import { mapExerciseMutationError } from "@/features/exercises/model/error-mapping";
import { parseWorkoutExercisePrescription } from "@/features/workouts/model/workout-exercise-prescription";
import {
  ExercisePickerList,
  type ExercisePickerSource,
} from "@/features/workouts/ui/exercise-picker-list";
import { mapWorkoutMutationError } from "@/features/workouts/model/error-mapping";
import { WorkoutExercisePrescriptionFields } from "@/features/workouts/ui/workout-exercise-prescription-fields";
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { workoutsApi } from "@/lib/api/clients/workouts-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface AddExerciseToWorkoutFormProps {
  workoutId: string;
}

export function AddExerciseToWorkoutForm({ workoutId }: AddExerciseToWorkoutFormProps) {
  const router = useRouter();
  const [isOpen, setIsOpen] = useState(false);
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [selectedExerciseId, setSelectedExerciseId] = useState("");
  const [activeSource, setActiveSource] = useState<ExercisePickerSource>("all");
  const [search, setSearch] = useState("");
  const [sets, setSets] = useState("3");
  const [repetitions, setRepetitions] = useState("10");
  const [loadKg, setLoadKg] = useState("");
  const [restSeconds, setRestSeconds] = useState("60");
  const [trainerNote, setTrainerNote] = useState("");
  const [isLoadingExercises, setIsLoadingExercises] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const selectedExercise = exercises.find((exercise) => exercise.id === selectedExerciseId) ?? null;

  function resetPrescriptionFields() {
    setSelectedExerciseId("");
    setSets("3");
    setRepetitions("10");
    setLoadKg("");
    setRestSeconds("60");
    setTrainerNote("");
  }

  async function openPicker() {
    setIsOpen(true);
    setFormError(null);

    if (exercises.length > 0 || isLoadingExercises) {
      return;
    }

    setIsLoadingExercises(true);
    try {
      setExercises(await exercisesApi.getExercises("all"));
    } catch (error) {
      setFormError(mapExerciseMutationError(error));
    } finally {
      setIsLoadingExercises(false);
    }
  }

  function closePicker() {
    setIsOpen(false);
    setFormError(null);
    resetPrescriptionFields();
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedExerciseId) {
      setFormError("Виберіть вправу.");
      return;
    }

    const prescription = parseWorkoutExercisePrescription({
      sets,
      repetitions,
      loadKg,
      restSeconds,
      trainerNote,
    });

    if (!prescription.payload) {
      setFormError(prescription.error);
      return;
    }

    setIsSubmitting(true);
    setFormError(null);

    try {
      await workoutsApi.addExercise(workoutId, {
        exerciseId: selectedExerciseId,
        ...prescription.payload,
      });
      resetPrescriptionFields();
      setIsOpen(false);
      router.refresh();
    } catch (error) {
      setFormError(mapWorkoutMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="space-y-4">
      <button
        type="button"
        onClick={isOpen ? closePicker : openPicker}
        disabled={isSubmitting}
        className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isLoadingExercises ? "Завантажуємо..." : isOpen ? "Закрити додавання" : "Додати вправу"}
      </button>

      {isOpen ? (
        <form
          onSubmit={handleSubmit}
          className="space-y-5 rounded-2xl border border-border bg-white px-5 py-5"
        >
          <ExercisePickerList
            exercises={exercises}
            activeSource={activeSource}
            search={search}
            selectedExerciseId={selectedExerciseId}
            isLoading={isLoadingExercises}
            isSubmitting={isSubmitting}
            onActiveSourceChange={setActiveSource}
            onSearchChange={setSearch}
            onSelectExercise={setSelectedExerciseId}
          />

          <WorkoutExercisePrescriptionFields
            idPrefix="add-workout-exercise"
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

          {selectedExercise ? (
            <p className="text-sm text-muted">
              Обрано: <span className="font-medium text-foreground">{selectedExercise.name}</span>
            </p>
          ) : null}

          <FormAlert message={formError} />

          <div className="flex flex-col gap-3 sm:flex-row">
            <button
              type="submit"
              disabled={isSubmitting || isLoadingExercises || !selectedExerciseId}
              className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              {isSubmitting ? "Додаємо..." : "Додати до тренування"}
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
