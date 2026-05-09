"use client";

import { type FormEvent, useState } from "react";
import { useRouter } from "next/navigation";

import type { Exercise } from "@/entities/exercise/model/types";
import { mapExerciseMutationError } from "@/features/exercises/model/error-mapping";
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface ExerciseActionsProps {
  exercise: Exercise;
}

export function ExerciseActions({ exercise }: ExerciseActionsProps) {
  const router = useRouter();
  const [isEditing, setIsEditing] = useState(false);
  const [name, setName] = useState(exercise.name);
  const [description, setDescription] = useState(exercise.description);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  if (!exercise.isEditable) {
    return null;
  }

  async function handleUpdate() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await exercisesApi.updateExercise(exercise.id, {
        name,
        description,
        mediaUrl: exercise.mediaUrl,
        muscleGroup: exercise.muscleGroup,
        equipment: exercise.equipment,
      });
      setIsEditing(false);
      router.refresh();
    } catch (error) {
      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await exercisesApi.deleteExercise(exercise.id);
      router.refresh();
    } catch (error) {
      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleEditToggle() {
    setIsEditing((current) => {
      const next = !current;

      if (!next) {
        setName(exercise.name);
        setDescription(exercise.description);
      }

      return next;
    });
    setSubmitError(null);
  }

  function handleEditSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void handleUpdate();
  }

  return (
    <div className="w-full space-y-3 md:w-auto">
      <div className="flex shrink-0 flex-wrap gap-2 md:justify-end">
        <button
          type="button"
          onClick={handleEditToggle}
          disabled={isSubmitting}
          className="rounded-full border border-border px-4 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isEditing ? "Скасувати" : "Редагувати"}
        </button>
        <button
          type="button"
          onClick={handleDelete}
          disabled={isSubmitting}
          className="rounded-full border border-red-200 px-4 py-2 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Обробка..." : "Видалити"}
        </button>
      </div>

      <FormAlert message={submitError} />

      {isEditing ? (
        <form
          onSubmit={handleEditSubmit}
          className="space-y-3 rounded-2xl border border-border bg-surface px-4 py-4"
        >
          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor={`exercise-name-${exercise.id}`}>
              Назва
            </label>
            <input
              id={`exercise-name-${exercise.id}`}
              value={name}
              onChange={(event) => setName(event.target.value)}
              disabled={isSubmitting}
              className={fieldInputClassName}
            />
          </div>

          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor={`exercise-description-${exercise.id}`}>
              Опис
            </label>
            <textarea
              id={`exercise-description-${exercise.id}`}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              disabled={isSubmitting}
              rows={4}
              className={`${fieldInputClassName} resize-y`}
            />
          </div>

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
