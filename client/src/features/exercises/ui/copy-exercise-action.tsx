"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import { ExerciseSource, type Exercise } from "@/entities/exercise/model/types";
import { mapExerciseMutationError } from "@/features/exercises/model/error-mapping";
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface CopyExerciseActionProps {
  exercise: Exercise;
  isAlreadyCopied?: boolean;
}

export function CopyExerciseAction({
  exercise,
  isAlreadyCopied = false,
}: CopyExerciseActionProps) {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isCopied, setIsCopied] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  if (exercise.source !== ExerciseSource.Platform) {
    return null;
  }

  const isCopyUnavailable = isSubmitting || isCopied || isAlreadyCopied;
  const buttonLabel =
    isCopied || isAlreadyCopied
      ? "Вже у моїх вправах"
      : isSubmitting
        ? "Копіюємо..."
        : "Додати до моїх вправ";

  async function handleCopy() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await exercisesApi.copyToMyLibrary(exercise.id);
      setIsCopied(true);
      router.refresh();
    } catch (error) {
      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="w-full space-y-2 md:w-auto">
      <button
        type="button"
        onClick={handleCopy}
        disabled={isCopyUnavailable}
        className={`rounded-full border px-4 py-2 text-sm font-medium transition disabled:cursor-not-allowed ${
          isCopied || isAlreadyCopied
            ? "border-emerald-200 bg-emerald-50 text-emerald-800"
            : "border-accent text-accent hover:bg-emerald-50 disabled:opacity-70"
        }`}
      >
        {buttonLabel}
      </button>
      <FormAlert message={submitError} />
    </div>
  );
}
