"use client";

import { type SubmitEvent, useState } from "react";
import { useRouter } from "next/navigation";

import { mapWorkoutMutationError } from "@/features/workouts/model/error-mapping";
import { workoutsApi } from "@/lib/api/clients/workouts-api";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";
import { FormAlert } from "@/shared/forms/form-alert";

interface CreateWorkoutFormProps {
  onCreated: () => void;
  onCancel: () => void;
}

export function CreateWorkoutForm({ onCreated, onCancel }: CreateWorkoutFormProps) {
  const router = useRouter();
  const [name, setName] = useState("");
  const [nameError, setNameError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function resetForm() {
    setName("");
    setNameError(null);
    setSubmitError(null);
  }

  function handleCancel() {
    resetForm();
    onCancel();
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedName = name.trim();
    if (!trimmedName) {
      setNameError("Вкажіть назву тренування.");
      return;
    }

    setIsSubmitting(true);
    setNameError(null);
    setSubmitError(null);

    try {
      await workoutsApi.createWorkout({ name: trimmedName });
      resetForm();
      router.refresh();
      onCreated();
    } catch (error) {
      setSubmitError(mapWorkoutMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5"
    >
      <div className="space-y-2">
        <label className={fieldLabelClassName} htmlFor="create-workout-name">
          Назва
        </label>
        <input
          id="create-workout-name"
          value={name}
          onChange={(event) => {
            setName(event.target.value);
            if (nameError) {
              setNameError(null);
            }
          }}
          disabled={isSubmitting}
          required
          maxLength={200}
          aria-invalid={nameError ? "true" : "false"}
          aria-describedby={nameError ? "create-workout-name-error" : undefined}
          className={fieldInputClassName}
        />
        {nameError ? (
          <p id="create-workout-name-error" className="text-sm text-red-700">
            {nameError}
          </p>
        ) : null}
      </div>

      <FormAlert message={submitError} />

      <div className="flex flex-col gap-3 sm:flex-row">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Створюємо..." : "Створити"}
        </button>
        <button
          type="button"
          onClick={handleCancel}
          disabled={isSubmitting}
          className="rounded-full border border-border px-5 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          Скасувати
        </button>
      </div>
    </form>
  );
}
